using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CustomGoogleClient.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult ShowData()
    {
      AuthenticationResult result = Session["AuthResult"] as AuthenticationResult;
      return View(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public ActionResult Login()
    {
      return new ExternalLoginResult("google", Url.Action("LoginCallback"));
    }

    [AllowAnonymous]
    public ActionResult LoginCallback(string returnUrl)
    {
      AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("LoginCallback", new { ReturnUrl = returnUrl }));
      if (!result.IsSuccessful)
      {
        return RedirectToAction("LoginFailure");
      }

      FormsAuthentication.SetAuthCookie(result.UserName, false);
      Session["AuthResult"] = result;
      return RedirectToAction("ShowData");
    }

    [AllowAnonymous]
    public ActionResult LoginFailure()
    {
      return View();
    }
  }

  internal class ExternalLoginResult : ActionResult
  {
    public ExternalLoginResult(string provider, string returnUrl)
    {
      Provider = provider;
      ReturnUrl = returnUrl;
    }

    public string Provider { get; private set; }
    public string ReturnUrl { get; private set; }

    public override void ExecuteResult(ControllerContext context)
    {
      OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
    }
  }
}
