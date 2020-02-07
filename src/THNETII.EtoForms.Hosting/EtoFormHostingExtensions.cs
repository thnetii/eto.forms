using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using THNETII.EtoForms.Hosting;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods for using the Eto.Forms library in a
    /// generic host.
    /// </summary>
    public static class EtoFormHostingExtensions
    {
        /// <summary>
        /// Enables Eto.Forms support, builds and starts the host and waits for
        /// the main form of the specified type to close to shut down.
        /// <para>
        /// The Form type <typeparamref name="TForm"/> is added as a singleton
        /// service to the service provider if not already configured.
        /// </para>
        /// </summary>
        /// <typeparam name="TForm">The type of the main form class.</typeparam>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to configure.</param>
        /// <param name="cancelToken">An optional <see cref="CancellationToken"/> that can be used to close the application.</param>
        public static void RunEtoForm<TForm>(this IHostBuilder hostBuilder,
            CancellationToken cancelToken = default)
            where TForm : Eto.Forms.Form
        {
            if (hostBuilder is null)
                throw new ArgumentNullException(nameof(hostBuilder));

            var platform = Eto.Platform.Detect;
            using var application = new Eto.Forms.Application(platform);

            using var host = hostBuilder
                .ConfigureServices((_, services) =>
                {
                    services.AddEtoFormsHost(application);
                    services.TryAddSingleton<TForm>();
                    services.Configure<EtoFormsOptions>(opts => opts.MainForm = typeof(TForm));
                })
                .Build();

            host.Start();

            var options = host.Services
                .GetRequiredService<IOptions<EtoFormsOptions>>().Value;
            object form = null;
            if (options.MainForm is Type formType)
                form = ActivatorUtilities.GetServiceOrCreateInstance(
                    host.Services, formType);

            using var cancelReg = cancelToken.Register(obj =>
            {
                var app = (Eto.Forms.Application)obj;
                app.Quit();
            }, application);

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

            var hostLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            hostLifetime.StopApplication();
            host.WaitForShutdown();
        }
    }
}
