using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using System;

using THNETII.EtoForms.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EtoFormsServiceCollectionExtensions
    {
        public static IServiceCollection AddEtoFormsHost(
            this IServiceCollection services, Eto.Forms.Application application)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (application is null)
                throw new ArgumentNullException(nameof(application));

            services.AddSingleton(application.Platform);
            services.AddSingleton(application);
            services.AddSingleton<IHostLifetime, EtoFormsApplicationLifetime>();

            services.AddOptions<EtoFormsOptions>()
                .Configure<IServiceProvider>((opts, sp) =>
                {
                    var con = sp.GetService<ConsoleLifetimeOptions>();
                    opts.SuppressStatusMessages = con?.SuppressStatusMessages ?? false;
                })
                .Validate(opts =>
                {
                    opts.Validate();
                    return true;
                });

            return services;
        }

        public static IServiceCollection AddEtoFormsHost<TForm>(
            this IServiceCollection services, Eto.Forms.Application application)
            where TForm : Eto.Forms.Form
        {
            AddEtoFormsHost(services, application);

            services.TryAddSingleton<TForm>();
            services.AddOptions<EtoFormsOptions>()
                .Configure(opts => opts.MainForm = typeof(TForm));

            return services;
        }
    }
}
