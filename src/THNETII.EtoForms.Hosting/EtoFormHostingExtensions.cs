using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;
using System.Threading;
using System.Threading.Tasks;
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
        /// <returns>A Task that is completed when the main form is closed or <paramref name="cancelToken"/> is triggered.</returns>
        public static async Task RunEtoFormAsync<TForm>(this IHostBuilder hostBuilder,
            CancellationToken cancelToken = default)
            where TForm : Eto.Forms.Form
        {
            if (hostBuilder is null)
                throw new ArgumentNullException(nameof(hostBuilder));

            using var host = hostBuilder.ConfigureServices((_, services) =>
                {
                    services.AddEtoFormsHost();
                    services.Configure<EtoFormsOptions>(opts => opts.MainForm = typeof(TForm));
                })
                .Build();

            await host.RunAsync(cancelToken).ConfigureAwait(false);
        }
    }
}
