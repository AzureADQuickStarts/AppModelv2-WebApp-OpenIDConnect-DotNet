using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AppModelv2_WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        /// <summary>
        /// Add user's claims to viewbag
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            var userClaims = User.Identity as System.Security.Claims.ClaimsIdentity;

            // You get the user’s first and last name below:
            ViewBag.Name = userClaims?.FindFirst("name")?.Value;

            // The subject/ NameIdentifier claim can be used to uniquely identify the user across the web
            ViewBag.Subject = userClaims?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // TenantId is the unique Tenant Id - which represents an organization in Azure AD
            ViewBag.TenantId = userClaims?.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

            // You can also call Microsoft Graph (with incremental consent)
            try
            { 
                var me = await this.GetGraphServiceClient().Me.GetAsync();
                ViewBag.Username = me.DisplayName;
            }
            catch (ServiceException graphEx) when (graphEx.InnerException is MicrosoftIdentityWebChallengeUserException)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                return View();
            }

            return View();
        }
    }
}