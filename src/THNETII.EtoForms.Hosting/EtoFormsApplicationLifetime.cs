using Eto.Forms;
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
        private readonly Application application;

        private CancellationTokenRegistration startingRegistration;
        private CancellationTokenRegistration stoppingRegistration;

        public IHostApplicationLifetime ApplicationLifetime { get; }
        private IHostEnvironment Environment { get; }
        private ILogger Logger { get; }

        public EtoFormsApplicationLifetime(
            IServiceProvider serviceProvider,
            IOptions<EtoFormsOptions> options,
            Eto.Forms.Application application,
            IHostApplicationLifetime hostapplifetime,
            IHostEnvironment environment,
            ILoggerFactory loggerFactory
            ) : base()
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            this.application = application ?? throw new ArgumentNullException(nameof(application));

            ApplicationLifetime = hostapplifetime ?? throw new ArgumentNullException(nameof(hostapplifetime));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            Logger = loggerFactory?.CreateLogger("THNETII.EtoForms.Hosting.Lifetime")
                ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }

        public Task WaitForStartAsync(CancellationToken cancelToken)
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

            return Task.CompletedTask;
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
        }

        public Task StopAsync(CancellationToken cancelToken)
        {
            application.Quit();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            startingRegistration.Dispose();
            stoppingRegistration.Dispose();
        }
    }
}
