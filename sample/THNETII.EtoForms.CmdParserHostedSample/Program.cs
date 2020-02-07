using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;

using THNETII.EtoForms.Hosting;

namespace THNETII.EtoForms.CmdParserHostedSample
{
    public static class Program
    {
        public static ICommandHandler Handler { get; } = CommandHandler.Create(
        (IHost host, CancellationToken cancelToken) =>
        {
            var application = host.Services.GetRequiredService<Eto.Forms.Application>();

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
        });

        [STAThread]
        public static int Main(string[] args)
        {
            using var application = new Eto.Forms.Application(Eto.Platform.Detect);
            var rootCommand = new RootCommand() { Handler = Handler };
            var cmdParser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseHost(Host.CreateDefaultBuilder, hostBuilder =>
                {
                    hostBuilder.ConfigureServices(services =>
                    {
                        services.AddEtoFormsHost(application);
                        services.TryAddSingleton<MainForm>();
                        services.Configure<EtoFormsOptions>(opts => opts.MainForm = typeof(MainForm));
                    });
                })
                .Build();

            return cmdParser.InvokeAsync(args).ConfigureAwait(true)
                .GetAwaiter().GetResult();
        }
    }
}
