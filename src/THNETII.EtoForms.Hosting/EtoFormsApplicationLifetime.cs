using Microsoft.Extensions.Hosting;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace THNETII.EtoForms.Hosting
{
    public class EtoFormsApplicationLifetime : IHostLifetime
    {
        private TaskCompletionSource<object> completion;
        private Thread thread;

        public EtoFormsApplicationLifetime(Eto.Forms.Application application = null)
        {
            Application = application ?? new Eto.Forms.Application();
        }

        public Eto.Forms.Application Application { get; }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<object>();
            var thread = new Thread(RunApplication);
            _ = thread.TrySetApartmentState(ApartmentState.STA);
            thread.Start();

            (this.completion, this.thread) = (completion, thread);

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
            var (th, completion) = (thread, this.completion);
            using (var forceStopReg = cancellationToken.Register(arg => ForceStopThread((Thread)arg), th))
                Application.Quit();

            return completion.Task ?? Task.CompletedTask;
        }

        [SuppressMessage("Usage", "PC001: API not supported on all platforms")]
        private void ForceStopThread(Thread thread) => thread.Abort();
    }
}
