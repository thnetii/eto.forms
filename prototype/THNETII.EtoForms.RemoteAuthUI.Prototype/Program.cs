using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace THNETII.EtoForms.RemoteAuthUI
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var etoPlatform = Eto.Platform.Detect;
            var etoApplication = new Eto.Forms.Application(etoPlatform);

            //var taskCompletionSource = new TaskCompletionSource<IHost>();
            //var hostingThread = new Thread(o => RunHost(o as TaskCompletionSource<IHost>));
            //hostingThread.Start(taskCompletionSource);
            //var host = taskCompletionSource.Task.GetAwaiter().GetResult();

            var host = CreateHost();
            host.Start();

            etoApplication.Run(new MainForm(host));

            host.StopAsync().GetAwaiter().GetResult();
        }

        private static void CreateAndRunHost(TaskCompletionSource<IHost> taskCompletionSource)
        {
            IHost host = CreateHost();
            taskCompletionSource.SetResult(host);
            host.Run();
        }

        private static IHost CreateHost()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(c => c.AddUserSecrets(typeof(Program).Assembly, optional: true))
                .ConfigureServices((ctx, services) =>
                {
                    services.AddAuthentication()
                    .AddTwitch(options =>
                    {
                        ctx.Configuration.GetSection(nameof(AspNet.Security.OAuth.Twitch)).Bind(options);
#if DEBUG
                        options.ForceVerify = true;
#endif
                        options.SaveTokens = true;
                    });
                })
                .Build();
            return host;
        }
    }
}
