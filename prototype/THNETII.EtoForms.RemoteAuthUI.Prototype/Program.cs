using System;
using System.Threading;
using System.Threading.Tasks;

using AspNet.Security.OAuth.Twitch;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace THNETII.EtoForms.RemoteAuthUI
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var etoPlatform = Eto.Platform.Detect;
            var etoApplication = new Eto.Forms.Application(etoPlatform);

            var hostThread = new Thread(Run);
            hostThread.Start(etoApplication);

            etoApplication.Run();

            hostThread.Join();
        }

        private static void Run(object arg)
        {
            var application = (Eto.Forms.Application)arg;

            CreateHost(application).Run();
            application.Quit();
        }

        private static IHost CreateHost(Eto.Forms.Application etoApplication) =>
            Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(config =>
            {
                config.AddUserSecrets<Application>(optional: true);
            })
#if DEBUG
            .ConfigureLogging(logging => logging.AddDebug())
#endif // DEBUG
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton(etoApplication);
                services.AddAuthentication().AddTwitch(options =>
                {
                    options.ClientId = ctx.Configuration[ConfigurationPath.Combine(nameof(AspNet.Security.OAuth.Twitch), nameof(TwitchAuthenticationOptions.ClientId))];
                    options.ClientSecret = "urn:twitch:clientsecret";
                    //options.ClientSecret = ctx.Configuration[ConfigurationPath.Combine(nameof(AspNet.Security.OAuth.Twitch), nameof(TwitchAuthenticationOptions.ClientSecret))];
#if DEBUG
                    options.ForceVerify = true;
#endif // DEBUG
                });
                services.AddHostedService<Application>();
            })
            .Build();
    }
}
