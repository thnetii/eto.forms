using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace THNETII.EtoForms.RemoteAuthUI
{
    public class Application : BackgroundService
    {
        public Application(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var localhostUri = new Uri("https://localhost/");

            var handlers = ServiceProvider.GetRequiredService<IAuthenticationHandlerProvider>();
            var schemes = ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
            foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
            {
                var httpCtx = new DefaultHttpContext();
                httpCtx.Request.Protocol = "HTTP/1.1";
                httpCtx.Request.Method = HttpMethods.Get;
                httpCtx.Request.Scheme = localhostUri.Scheme;
                httpCtx.Request.Host = HostString.FromUriComponent(localhostUri);
                httpCtx.Request.Path = PathString.FromUriComponent(localhostUri);
                httpCtx.RequestServices = ServiceProvider;
                //httpCtx.ServiceScopeFactory = ServiceProvider.GetService<IServiceScopeFactory>();
                httpCtx.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
                {
                    OriginalPath = httpCtx.Request.Path,
                    OriginalPathBase = httpCtx.Request.PathBase
                });

                var handler = await handlers.GetHandlerAsync(httpCtx, scheme.Name);
                if (!IsRemoteAuthenticationHandler(handler, out RemoteAuthenticationOptions options))
                    continue;
                var authProperties = new AuthenticationProperties();
                await handler.ChallengeAsync(authProperties);

                var challengeHeaders = new ResponseHeaders(httpCtx.Response.Headers);
                var challengeUri = challengeHeaders.Location;

                var redirectUri = new Uri($"{httpCtx.Request.Scheme}://{httpCtx.Request.Host}{httpCtx.Request.PathBase}{options.CallbackPath}");
                //var taskSource = new TaskCompletionSource<Uri>();
                //var uiArguments = new object[] {
                //    challengeUri,
                //    new Uri(localhostUri, options.CallbackPath.ToUriComponent()),
                //    ServiceProvider,
                //    taskSource
                //};
                //var uiThread = new Thread(ExecuteForm);
                //var staSet = uiThread.TrySetApartmentState(ApartmentState.STA);
                //uiThread.Start(uiArguments);
                //redirectUri = await taskSource.Task;
                redirectUri = await ExecuteRemoteAuthentication(challengeUri, redirectUri);

                httpCtx.Request.Protocol = "HTTP/1.1";
                httpCtx.Request.Method = HttpMethods.Get;
                httpCtx.Request.Scheme = redirectUri.Scheme;
                httpCtx.Request.Host = HostString.FromUriComponent(redirectUri);
                httpCtx.Request.Path = PathString.FromUriComponent(redirectUri);
                httpCtx.Request.QueryString = new QueryString(redirectUri.Query);
                httpCtx.RequestServices = ServiceProvider;
                //httpCtx.ServiceScopeFactory = ServiceProvider.GetService<IServiceScopeFactory>();

                var isAuthHandled = await (handler as IAuthenticationRequestHandler).HandleRequestAsync();
                //var authResult = await handler.AuthenticateAsync();
            }

            ServiceProvider.GetService<IHostApplicationLifetime>()?
                .StopApplication();
        }

        private Task<Uri> ExecuteRemoteAuthentication(Uri challangeUri, Uri redirectUri)
        {
            var etoApplication = ServiceProvider.GetRequiredService<Eto.Forms.Application>();
            
            return etoApplication.InvokeAsync(() =>
            {
                var redirectPart = redirectUri.GetLeftPart(UriPartial.Path);

                var etoWebView = new Eto.Forms.WebView { Url = challangeUri };
                var etoRemoteAuthDlg = new Eto.Forms.Dialog<Uri> { Content = etoWebView };

                var etoWebLoadedHandler = new EventHandler<Eto.Forms.WebViewLoadedEventArgs>((sender, e) =>
                {
                    if (e.Uri.GetLeftPart(UriPartial.Path) != redirectPart)
                        return;

                    etoRemoteAuthDlg.Close(e.Uri);
                });
                //etoWebView.Navigated += etoWebLoadedHandler;
                //etoWebView.DocumentLoaded += etoWebLoadedHandler;
                etoWebView.DocumentLoading += (sender, e) => etoWebLoadedHandler(sender, e);
                etoWebView.DocumentTitleChanged += (sender, e) =>
                {
                    var parent = (sender as Eto.Forms.WebView)?.ParentWindow;
                    if (parent != null)
                        parent.Title = e.Title;
                };

                return etoRemoteAuthDlg.ShowModal();
            });
        }

        [STAThread]
        private void ExecuteForm(object arg)
        {
            object[] args = arg as object[];
            Uri challengeUri = args[0] as Uri;
            string redirectUri = (args[1] as Uri).GetLeftPart(UriPartial.Path);
            var serviceProvider = args[2] as IServiceProvider;
            var taskSource = args[3] as TaskCompletionSource<Uri>;

            var logger = serviceProvider.GetService<ILogger<Eto.Forms.WebView>>();
            var etoApplication = serviceProvider.GetRequiredService<Eto.Forms.Application>();
            var etoForm = new Eto.Forms.Dialog<Uri>
            {
                Resizable = true,
                Maximizable = false,
                Minimizable = false,
            };
            var etoWebView = new Eto.Forms.WebView
            {
                Url = challengeUri,
                BrowserContextMenuEnabled = true
            };

            var etoWebViewLoadedHandler = new EventHandler<Eto.Forms.WebViewLoadedEventArgs>(OnWebViewLoadedEvent);
            etoWebView.Navigated += etoWebViewLoadedHandler;
            etoWebView.DocumentLoaded += etoWebViewLoadedHandler;
            etoWebView.DocumentLoading += (sender, e) => etoWebViewLoadedHandler(sender, e);
            etoWebView.DocumentTitleChanged += (sender, e) => etoForm.Title = e.Title;

            etoForm.Content = etoWebView;

            var etoResult = etoApplication.Invoke(() => etoForm.ShowModalAsync());

            taskSource.SetResult(etoForm.Result);

            void OnWebViewLoadedEvent(object sender, Eto.Forms.WebViewLoadedEventArgs e)
            {
                logger?.LogDebug($"Navigation ocurred: {{{nameof(e.Uri)}}}", e.Uri);

                if (e.Uri.GetLeftPart(UriPartial.Path) != redirectUri)
                    return;

                etoApplication.Invoke(() =>
                {
                    var msgStringBuilder = new StringBuilder($"Intercepted Remote Authentication Callback: {{{nameof(e.Uri)}}}");
                    var msgParams = new List<object> { e.Uri.GetLeftPart(UriPartial.Path) };
                    foreach (var kvp in QueryHelpers.ParseNullableQuery(e.Uri.Query))
                    {
                        msgStringBuilder.Append($", {{{kvp.Key}}}");
                        msgParams.Add(kvp.Value);
                    }
                    logger?.LogInformation(msgStringBuilder.ToString(), msgParams.ToArray());

                    etoForm.Close(e.Uri);
                });
            }
        }

        private static bool IsRemoteAuthenticationHandler(IAuthenticationHandler handler, out RemoteAuthenticationOptions options)
        {
            for (var t = handler?.GetType(); t is Type; t = t.BaseType)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RemoteAuthenticationHandler<>))
                {
                    var optionsProperty = t.GetProperty(nameof(RemoteAuthenticationHandler<RemoteAuthenticationOptions>.Options));
                    options = (RemoteAuthenticationOptions)optionsProperty.GetValue(handler);
                    return true;
                }
            }

            options = null;
            return false;
        }
    }
}
