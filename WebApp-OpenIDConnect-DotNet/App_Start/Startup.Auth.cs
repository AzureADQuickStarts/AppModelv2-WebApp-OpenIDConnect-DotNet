using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Configuration;
using System.Globalization;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;

namespace TodoList_WebApp
{
    public partial class Startup
    {
        public static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        public static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    // The `Authority` represents the v2.0 endpoint - https://login.microsoftonline.com/common/v2.0
                    // The `Scope` describes the permissions that your app will need.  See https://azure.microsoft.com/documentation/articles/active-directory-v2-scopes/
                    // In a real application you could use issuer validation for additional checks, like making sure the user's organization has signed up for your app, for instance.

                    ClientId = clientId,
                    Authority = String.Format(CultureInfo.InvariantCulture, aadInstance, "common", "/v2.0"),
                    RedirectUri = redirectUri,
                    Scope = "openid email profile",
                    ResponseType = "id_token",
                    PostLogoutRedirectUri = redirectUri,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = OnAuthenticationFailed,
                    }
                });
        }

        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();
            notification.Response.Redirect("/Error?message=" + notification.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
