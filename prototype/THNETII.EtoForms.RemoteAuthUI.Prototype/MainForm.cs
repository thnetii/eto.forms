using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace THNETII.EtoForms.RemoteAuthUI
{
    public class MainForm : Eto.Forms.Form
    {
        public MainForm(IHost host) : base()
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Content = new Eto.Forms.StackLayout(
                new Eto.Forms.StackLayoutItem(new Eto.Forms.Button(OnButtonClick) { Text = "Login" })
                )
            {
                Padding = new Eto.Drawing.Padding(20),
                VerticalContentAlignment = Eto.Forms.VerticalAlignment.Center,
                HorizontalContentAlignment = Eto.Forms.HorizontalAlignment.Center
            };
            ClientSize = new Eto.Drawing.Size(-1, -1);
        }

        private async void OnButtonClick(object sender, EventArgs e)
        {
            var button = sender as Eto.Forms.Button;
            try
            {
                if (button != null)
                    button.Enabled = false;

                var services = Host.Services;
                var handlers = services.GetRequiredService<IAuthenticationHandlerProvider>();
                var schemes = services.GetRequiredService<IAuthenticationSchemeProvider>();

                foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
                {
                    var httpCtx = new DefaultHttpContext { RequestServices = services };
                    var request = httpCtx.Request;
                    request.Protocol = "HTTP/1.1";
                    request.Method = HttpMethods.Get;
                    request.Scheme = pathBase.Scheme;
                    request.Host = HostString.FromUriComponent(pathBase);
                    request.Path = PathString.FromUriComponent(pathBase);
                    httpCtx.Features.Set<IAuthenticationFeature>(new AuthenticationFeature
                    {
                        OriginalPath = request.Path
                    });

                    var handler = await handlers.GetHandlerAsync(httpCtx, scheme.Name) as IAuthenticationRequestHandler;
                    if (!IsRemoteAuthenticationHandler(handler, out RemoteAuthenticationOptions options))
                        continue;
                    var authProperties = new AuthenticationProperties();
                    await handler.ChallengeAsync(authProperties);
                    var challengeHeaders = new ResponseHeaders(httpCtx.Response.Headers);
                    var challengeUri = challengeHeaders.Location;
                    var challengeQuery = QueryHelpers.ParseNullableQuery(challengeUri?.Query);

                    var redirectUri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}{options.CallbackPath}");
                    var redirectMatch = redirectUri.GetLeftPart(UriPartial.Path);
                    var redirectWebView = new Eto.Forms.WebView
                    {
                        Url = challengeUri,
                        Size = new Eto.Drawing.Size(-1, -1)
                    };
                    var redirectDialog = new Eto.Forms.Dialog<Uri>
                    {
                        Content = redirectWebView,
                        Resizable = true,
                        Size = new Eto.Drawing.Size(-1, -1)
                    };
                    redirectWebView.Navigated += (sender, e) =>
                    {
                        if (e.Uri.GetLeftPart(UriPartial.Path) != redirectMatch)
                            return;
                        redirectDialog.Close(e.Uri);
                    };
                    redirectWebView.DocumentTitleChanged += (sender, e) => redirectDialog.Title = e.Title;
                    redirectUri = redirectDialog.ShowModal();

                    request.Scheme = pathBase.Scheme;
                    request.Host = HostString.FromUriComponent(redirectUri);
                    request.Path = PathString.FromUriComponent(redirectUri);
                    request.QueryString = QueryString.FromUriComponent(redirectUri);
                    var remoteAuthHeaders = new RequestHeaders(request.Headers)
                    {
                        Cookie = challengeHeaders.SetCookie.Select(c => new Microsoft.Net.Http.Headers.CookieHeaderValue(c.Name, c.Value)).ToList()
                    };

                    TaskCompletionSource<IEnumerable<AuthenticationToken>> tokenSource = new TaskCompletionSource<IEnumerable<AuthenticationToken>>();
                    options.Events.OnTicketReceived = ticket =>
                    {
                        tokenSource.SetResult(ticket.Properties.GetTokens());
                        ticket.HandleResponse();
                        return Task.CompletedTask;
                    };
                    var isAuthHandled = await handler.HandleRequestAsync();
                    if (isAuthHandled)
                    {
                        var tokens = await tokenSource.Task;

                    }

                    var response = httpCtx.Response;
                }
            }
            finally
            {
                if (button != null)
                    button.Enabled = true;
            }
        }

        private static readonly Uri pathBase = new Uri("https://localhost/");

        public IHost Host { get; }
        public CancellationTokenSource CancellationTokenSource { get; }

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
