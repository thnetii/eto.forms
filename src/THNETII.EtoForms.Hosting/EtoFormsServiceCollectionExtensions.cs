using THNETII.EtoForms.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EtoFormsServiceCollectionExtensions
    {
        public static IServiceCollection AddEtoFormsHost(IServiceCollection services)
        {
            services.AddSingleton(_ => Eto.Platform.Detect);
            services.AddSingleton<Eto.Forms.Application>();
            services.AddHostedService<EtoFormsApplicationHost>();

            return services;
        }
    }
}
