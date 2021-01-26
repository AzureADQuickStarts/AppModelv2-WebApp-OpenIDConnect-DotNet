using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(AppModelv2_WebApp_OpenIDConnect_DotNet.Startup))]

namespace AppModelv2_WebApp_OpenIDConnect_DotNet
{
    public class Startup
    {
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];

        // RedirectUri is the URL where the user will be redirected to after they sign in.
        string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];

        // Tenant is the tenant ID (e.g. contoso.onmicrosoft.com, or 'common' for multi-tenant)
        static string tenant = System.Configuration.ConfigurationManager.AppSettings["Tenant"];

        // Authority is the URL for authority, composed by Microsoft identity platform endpoint and the tenant name (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0)
        string authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, System.Configuration.ConfigurationManager.AppSettings["Authority"], tenant);

        /// <summary>
        /// Configure OWIN to use OpenIdConnect 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            var openIdConnectAuthenticationOptions = new OpenIdConnectAuthenticationOptions
            {
                // Sets the ClientId, authority, RedirectUri as obtained from web.config
                ClientId = clientId,
                Authority = authority,
                RedirectUri = redirectUri,
                // PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it is using the home page
                PostLogoutRedirectUri = redirectUri,
                Scope = OpenIdConnectScope.OpenIdProfile,
                // ResponseType is set to request the code id_token - which contains basic information about the signed-in user
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                // OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to OnAuthenticationFailed method
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = OnAuthenticationFailed
                }
            };
            // prevents an error message IDX10205 when using multi tenant option: key="Tenant" value="common"
            // thanks to this article https://thomaslevesque.com/2018/12/24/multitenant-azure-ad-issuer-validation-in-asp-net-core/
            openIdConnectAuthenticationOptions.TokenValidationParameters.IssuerValidator = ValidateIssuerWithPlaceholder;

            app.UseOpenIdConnectAuthentication(openIdConnectAuthenticationOptions);
        }

        private static string ValidateIssuerWithPlaceholder(string issuer, SecurityToken token, TokenValidationParameters parameters)
        {
            // Accepts any issuer of the form "https://login.microsoftonline.com/{tenantid}/v2.0",
            // where tenantid is the tid from the token.
            var validIssuers = new List<string>();

            if (token is JwtSecurityToken jwt)
            {
                if (jwt.Payload.TryGetValue("tid", out var value) &&
                    value is string tokenTenantId)
                {
                    validIssuers = (parameters.ValidIssuers ?? Enumerable.Empty<string>())
                        .Append(parameters.ValidIssuer)
                        .Where(i => !string.IsNullOrEmpty(i)).ToList();

                    if (validIssuers.Any(i => i.Replace("{tenantid}", tokenTenantId) == issuer))
                        return issuer;
                }
            }

            // Recreate the exception that is thrown by default
            // when issuer validation fails
            var validIssuer = parameters.ValidIssuer ?? "null";
            var validIssuersString = parameters.ValidIssuers == null
                ? "null"
                : !parameters.ValidIssuers.Any()
                    ? "empty"
                    : string.Join(", ", parameters.ValidIssuers);
            string errorMessage = FormattableString.Invariant(
                $"IDX10205: Issuer validation failed. Issuer: '{issuer}'. Did not match: validationParameters.ValidIssuer: '{validIssuer}' or validationParameters.ValidIssuers: '{validIssuers}'.");

            throw new SecurityTokenInvalidIssuerException(errorMessage)
            {
                InvalidIssuer = issuer
            };
        }

        /// <summary>
        /// Handle failed authentication requests by redirecting the user to the home page with an error in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
