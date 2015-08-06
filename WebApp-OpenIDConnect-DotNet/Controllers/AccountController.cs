using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;

namespace TodoList_WebApp.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn()
        {
            // TODO: Send an OpenID Connect sign-in request.
        }

        // BUGBUG: Ending a session with the v2.0 endpoint is not yet supported.  Here, we just end the session with the web app.  
        public void SignOut()
        {
            // TODO: Send an OpenID Connect sign-out request.
        }
	}
}