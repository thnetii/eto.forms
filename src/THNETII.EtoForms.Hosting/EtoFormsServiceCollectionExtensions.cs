using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using System;

using THNETII.EtoForms.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EtoFormsServiceCollectionExtensions
    {
        public static IServiceCollection AddEtoFormsHost(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            
            services.AddSingleton(_ => Eto.Platform.Detect);
            services.AddSingleton<Eto.Forms.Application>();
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
    }
}
