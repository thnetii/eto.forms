using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

using THNETII.EtoForms.Hosting;
using THNETII.EtoForms.CommandLine;

namespace THNETII.EtoForms.CmdParserHostedSample
{
    public static class Program
    {
        public static ICommandHandler Handler { get; } =
            EtoFormsCommandHandler.Create();

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
