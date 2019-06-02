using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace THNETII.EtoForms.Hosting
{
    public class EtoFormsApplicationHost : IHostedService
    {
        private Thread thread;
        private TaskCompletionSource<object> completion;

        public EtoFormsApplicationHost(ILogger<EtoFormsApplicationHost> logger = null)
            : base()
        {
            Logger = logger ?? NullLogger<EtoFormsApplicationHost>.Instance;
        }

        public EtoFormsApplicationHost(IServiceProvider serviceProvider,
            ILogger<EtoFormsApplicationHost> logger = null)
            : this(logger)
        {
            Application = serviceProvider.GetService<Eto.Forms.Application>()
                ?? new Eto.Forms.Application();
        }

        public ILogger<EtoFormsApplicationHost> Logger { get; }
        public Eto.Forms.Application Application { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            completion = new TaskCompletionSource<object>();
            thread = new Thread(RunApplication);
            if (!thread.TrySetApartmentState(ApartmentState.STA))
            {
                Logger.LogInformation($"Unable to set GUI thread apartment state. Thread will run in the {{{nameof(ApartmentState)}}} apartment state", thread.GetApartmentState());
            }
            thread.Start();
            return Task.CompletedTask;
        }

        private void RunApplication()
        {
            try
            {
                Application.Run();
                completion.SetResult(null);
            }
            catch (ThreadAbortException) { completion.TrySetCanceled(); }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            var th = thread;
            using (var forceStopRegistration = cancellationToken.Register(arg => ForceStopThread(arg as Thread), th))
            {
                Application.Quit();
            }

            return completion.Task ?? Task.CompletedTask;
        }

        [SuppressMessage("Usage", "PC001: API not supported on all platforms")]
        private void ForceStopThread(Thread thread) => thread.Abort();
    }
}
