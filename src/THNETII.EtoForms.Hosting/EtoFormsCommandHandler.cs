using System;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace THNETII.EtoForms.Hosting
{
    /// <summary>
    /// Provides extensions for the <see cref="ICommandHandler"/> and <see cref="CommandHandler"/>
    /// types for use with Eto.Forms interop.
    /// </summary>
    public static class EtoFormsCommandHandler
    {
        private static readonly ICommandHandler defaultHandler =
            RunEtoFormsApplication(null);

        /// <summary>
        /// Creates a default Eto.Forms handler that runs a
        /// <see cref="Eto.Forms.Application"/> singleton instance on the main
        /// thread.
        /// </summary>
        /// <returns>A singleton <see cref="ICommandHandler"/> instance.</returns>
        public static ICommandHandler Create() => defaultHandler;

        /// <summary>
        /// Creates a default Eto.Forms handler that runs a
        /// <see cref="Eto.Forms.Application"/> singleton instance on the main
        /// thread, while scheduling the execution of a specified handler as
        /// a long-running task on a separate thread.
        /// </summary>
        /// <param name="handler">The handler to wrap, or <see langword="null"/> if no handler should be wrapped.</param>
        /// <returns>A <see cref="ICommandHandler"/> instance that runs both the <see cref="Eto.Forms.Application"/> and the <paramref name="handler"/>.</returns>
        public static ICommandHandler RunEtoFormsApplication(this ICommandHandler handler)
        {
            return CommandHandler.Create((IHost host, CancellationToken cancelToken) =>
            {
                var application = host.Services.GetRequiredService<Eto.Forms.Application>();
                var options = host.Services.GetRequiredService<IOptions<EtoFormsOptions>>().Value;

                object form = null;
                if (options.MainForm is Type formType)
                    form = ActivatorUtilities.GetServiceOrCreateInstance(
                        host.Services, formType);

                using var cancelReg = cancelToken.Register(obj =>
                {
                    var app = (Eto.Forms.Application)obj;
                    app.Quit();
                }, application);

                var handlerTask = handler is null ? null
                    : Task.Factory.StartNew(obj =>
                    {
                        var (handler, ctx) = (ValueTuple<ICommandHandler, InvocationContext>)obj;
                        return handler.InvokeAsync(ctx).GetAwaiter().GetResult();
                    },
                    (handler, host.Services.GetRequiredService<InvocationContext>()),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    Task.Factory.Scheduler);

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

                return handlerTask?.GetAwaiter().GetResult() ?? 0;
            });
        }
    }
}
