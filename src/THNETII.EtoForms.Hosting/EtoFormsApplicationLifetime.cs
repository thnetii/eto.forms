using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace THNETII.EtoForms.Hosting
{
    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Logging")]
    public sealed class EtoFormsApplicationLifetime : IHostLifetime, IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        private readonly EtoFormsOptions options;

        private Thread thread;
        private TaskCompletionSource<object> completion;
        private CancellationTokenRegistration startingRegistration;
        private CancellationTokenRegistration stoppingRegistration;

        public IHostApplicationLifetime ApplicationLifetime { get; }
        private IHostEnvironment Environment { get; }
        private ILogger Logger { get; }

        public EtoFormsApplicationLifetime(
            IServiceProvider serviceProvider,
            IOptions<EtoFormsOptions> options,
            IHostApplicationLifetime hostapplifetime,
            IHostEnvironment environment,
            ILoggerFactory loggerFactory
            ) : base()
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            ApplicationLifetime = hostapplifetime ?? throw new ArgumentNullException(nameof(hostapplifetime));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            Logger = loggerFactory?.CreateLogger("THNETII.EtoForms.Hosting.Lifetime")
                ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        public async Task WaitForStartAsync(CancellationToken cancelToken)
        {
            if (!options.SuppressStatusMessages)
            {
                startingRegistration = ApplicationLifetime.ApplicationStarted
                    .Register(state =>
                    {
                        var lf = (EtoFormsApplicationLifetime)state;
                        lf.OnApplicationStarted();
                    }, this);
                stoppingRegistration = ApplicationLifetime.ApplicationStopping
                    .Register(state =>
                    {
                        var lf = (EtoFormsApplicationLifetime)state;
                        lf.OnApplicationStopping();
                    }, this);
            }

            var startup = new TaskCompletionSource<object>();
            thread = new Thread(RunApplication)
            {
                Name = typeof(Eto.Forms.Application).FullName
            };
            completion = new TaskCompletionSource<object>();
            bool isStaThread = thread.TrySetApartmentState(ApartmentState.STA);
            if (!isStaThread)
            {
                Logger.LogInformation("Unable to set Eto.Forms.Application thread apartment state to STA.");
            }
            thread.Start(startup);

            using var cancelReg = cancelToken.Register(AbortThread, this);
            await startup.Task.ConfigureAwait(false);
        }

        [SuppressMessage("Design", "CA1031: Do not catch general exception types")]
        private void RunApplication(object parameter)
        {
            var startup = (TaskCompletionSource<object>)parameter;

            Eto.Forms.Application application;
            object form = null;
            try
            {
                application = serviceProvider
                    .GetRequiredService<Eto.Forms.Application>();
                if (options.MainForm is Type formType)
                    form = ActivatorUtilities.GetServiceOrCreateInstance(
                        serviceProvider, formType);
            }
            catch (ThreadAbortException)
            {
                startup.SetCanceled();
                return;
            }
            catch (Exception except)
            {
                startup.SetException(except);
                return;
            }
            startup.SetResult(null);

            try
            {
                switch (form)
                {
                    case Eto.Forms.Form mainForm:
                        application.Run(mainForm);
                        break;
                    case Eto.Forms.Dialog dialog:
                        application.Run(dialog);
                        break;
                    case null:
                        application.Run();
                        break;
                }

                completion.SetResult(null);
                if (!ApplicationLifetime.ApplicationStopping.IsCancellationRequested &&
                    !ApplicationLifetime.ApplicationStopped.IsCancellationRequested)
                    ApplicationLifetime.StopApplication();
            }
            catch (ThreadAbortException)
            {
                completion.SetCanceled();
            }
            catch (Exception except)
            {
                completion.SetException(except);
            }
        }

        private void OnApplicationStarted()
        {
            Logger.LogInformation("Application started. Close Main Form to shut down.");
            Logger.LogInformation("Hosting environment: {envName}", Environment.EnvironmentName);
            Logger.LogInformation("Content root path: {contentRoot}", Environment.ContentRootPath);
        }

        private void OnApplicationStopping()
        {
            Logger.LogInformation("Application is shutting down...");
            if (!completion.Task.IsCompleted)
                serviceProvider.GetRequiredService<Eto.Forms.Application>().Quit();
        }

        public async Task StopAsync(CancellationToken cancelToken)
        {
            using var cancelReg = cancelToken.Register(AbortThread, this);
            await completion.Task.ConfigureAwait(false);
        }

        [SuppressMessage("Usage", "PC001:API not supported on all platforms")]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
        private static void AbortThread(object state)
        {
            var lf = (EtoFormsApplicationLifetime)state;
            try { lf.thread.Abort(); }
            catch (Exception except)
            {
                lf.Logger.LogWarning(except, "Unable to force Eto.Forms.Application thread abortion.");
            }
        }

        public void Dispose()
        {
            startingRegistration.Dispose();
            stoppingRegistration.Dispose();
            if (thread.IsAlive)
                AbortThread(this);
        }
    }
}
