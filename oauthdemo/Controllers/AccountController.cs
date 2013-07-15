using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using oauthdemo.Filters;
using oauthdemo.Models;

namespace oauthdemo.Controllers
{
  [Authorize]
  //[InitializeSimpleMembership]
  public class AccountController : Controller
  {
    //
    // GET: /Account/Login

    [AllowAnonymous]
    public ActionResult Login(string returnUrl)
    {
      ViewBag.ReturnUrl = returnUrl;
      return View();
    }

    //
    // POST: /Account/Login

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult Login(LoginModel model, string returnUrl)
    {
      if (ModelState.IsValid)
      {
        FormsAuthentication.SetAuthCookie(model.UserName, false);
        return RedirectToLocal(returnUrl);
      }

      // If we got this far, something failed, redisplay form
      ModelState.AddModelError("", "The user name or password provided is incorrect.");
      return View(model);
    }

    //
    // POST: /Account/LogOff

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult LogOff()
    {
      FormsAuthentication.SignOut();

      return RedirectToAction("Index", "Home");
    }

    //
    // GET: /Account/Register

    [AllowAnonymous]
    public ActionResult Register()
    {
      return View();
    }

    //
    // POST: /Account/Register

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult Register(RegisterModel model)
    {
      if (ModelState.IsValid)
      {
        // Attempt to register the user
        try
        {
          using (UsersContext db = new UsersContext())
          {
            UserProfile user = new UserProfile() { UserName = model.UserName, Password = model.Password };
            db.UserProfiles.Add(user);
            db.SaveChanges();
          }
          FormsAuthentication.SetAuthCookie(model.UserName, false);
          return RedirectToAction("Index", "Home");
        }
        catch (MembershipCreateUserException e)
        {
          ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    //
    // POST: /Account/Disassociate

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Disassociate(string provider, string providerUserId)
    {
      string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
      ManageMessageId? message = null;

      // Only disassociate the account if the currently logged in user is the owner
      if (ownerAccount == User.Identity.Name)
      {
        using (UsersContext db = new UsersContext())
        {
          ExternalLoginProfile externalProfile = db.ExternalLoginProfiles.Include("User").FirstOrDefault(e => e.User.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
          if (externalProfile != null)
          {
            db.ExternalLoginProfiles.Remove(externalProfile);
            db.SaveChanges();
            message = ManageMessageId.RemoveLoginSuccess;
          }
        }
      }

      return RedirectToAction("Manage", new { Message = message });
    }

    //
    // GET: /Account/Manage

    public ActionResult Manage(ManageMessageId? message)
    {
      ViewBag.StatusMessage =
          message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
          : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
          : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
          : "";
      using (UsersContext db = new UsersContext())
      {
        UserProfile user = db.UserProfiles.FirstOrDefault(e => e.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
        ViewBag.HasLocalPassword = string.IsNullOrEmpty(user.Password);
      }
      ViewBag.ReturnUrl = Url.Action("Manage");
      return View();
    }

    //
    // POST: /Account/Manage

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Manage(LocalPasswordModel model)
    {
      bool hasLocalAccount = false;
      using (UsersContext db = new UsersContext())
      {
        UserProfile user = db.UserProfiles.FirstOrDefault(e => e.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
        hasLocalAccount = string.IsNullOrEmpty(user.Password);
      }
      ViewBag.HasLocalPassword = hasLocalAccount;
      ViewBag.ReturnUrl = Url.Action("Manage");
      if (hasLocalAccount)
      {
        if (ModelState.IsValid)
        {
          using (UsersContext db = new UsersContext())
          {
            UserProfile user = db.UserProfiles.FirstOrDefault(e => e.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            if (user != null && user.Password.Equals(model.OldPassword, StringComparison.OrdinalIgnoreCase))
            {
              user.Password = model.NewPassword;
              db.SaveChanges();
            }
            else
            {
              ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
            }
          }
          return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
        }
      }
      else
      {
        // User does not have a local password so remove any validation errors caused by a missing
        // OldPassword field
        ModelState state = ModelState["OldPassword"];
        if (state != null)
        {
          state.Errors.Clear();
        }

        if (ModelState.IsValid)
        {
          try
          {
            using (UsersContext db = new UsersContext())
            {
              UserProfile user = db.UserProfiles.FirstOrDefault(e => e.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
              user.Password = model.NewPassword;
              db.SaveChanges();
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
          }
          catch (Exception)
          {
            ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
          }
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    //
    // POST: /Account/ExternalLogin

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLogin(string provider, string returnUrl)
    {
      return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
    }

    //
    // GET: /Account/ExternalLoginCallback

    [AllowAnonymous]
    public ActionResult ExternalLoginCallback(string returnUrl)
    {
      AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
      if (!result.IsSuccessful)
      {
        return RedirectToAction("ExternalLoginFailure");
      }

      using (UsersContext db = new UsersContext())
      {
        ExternalLoginProfile externalLogin = db.ExternalLoginProfiles.Include("User").FirstOrDefault(p => p.Provider.Equals(result.Provider, StringComparison.OrdinalIgnoreCase) && p.ProviderUserId.Equals(result.ProviderUserId, StringComparison.OrdinalIgnoreCase));
        if (externalLogin != null)
        {
          FormsAuthentication.SetAuthCookie(externalLogin.User.UserName, false);
          return RedirectToLocal(returnUrl);
        }
      }

      if (User.Identity.IsAuthenticated)
      {
        // If the current user is logged in add the new account
        using (UsersContext db = new UsersContext())
        {
          ExternalLoginProfile externalLogin = db.ExternalLoginProfiles.FirstOrDefault(p => p.Provider.Equals(result.Provider, StringComparison.OrdinalIgnoreCase) && p.ProviderUserId.Equals(result.ProviderUserId, StringComparison.OrdinalIgnoreCase));
          if (externalLogin == null)
          {
            UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            externalLogin = new ExternalLoginProfile() { User = user, Provider = result.Provider, ProviderUserId = result.ProviderUserId };
            db.ExternalLoginProfiles.Add(externalLogin);
            db.SaveChanges();
          }
        }
        return RedirectToLocal(returnUrl);
      }
      else
      {
        // User is new, ask for their desired membership name
        string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
        ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
        ViewBag.ReturnUrl = returnUrl;
        return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
      }
    }

    //
    // POST: /Account/ExternalLoginConfirmation

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
    {
      string provider = null;
      string providerUserId = null;

      if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
      {
        return RedirectToAction("Manage");
      }

      if (ModelState.IsValid)
      {
        // Insert a new user into the database
        using (UsersContext db = new UsersContext())
        {
          UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase));
          // Check if user already exists
          if (user == null)
          {
            // Insert name into the profile table
            user = new UserProfile { UserName = model.UserName };
            db.UserProfiles.Add(user);
            ExternalLoginProfile externalLogin = new ExternalLoginProfile() { User = user, Provider = provider, ProviderUserId = providerUserId };
            db.ExternalLoginProfiles.Add(externalLogin);
            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(externalLogin.User.UserName, false);
            return RedirectToLocal(returnUrl);
          }
          else
          {
            ModelState.AddModelError("", "User name already exists. Please enter a different user name.");
          }
        }
      }

      ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
      ViewBag.ReturnUrl = returnUrl;
      return View(model);
    }

    //
    // GET: /Account/ExternalLoginFailure

    [AllowAnonymous]
    public ActionResult ExternalLoginFailure()
    {
      return View();
    }

    [AllowAnonymous]
    [ChildActionOnly]
    public ActionResult ExternalLoginsList(string returnUrl)
    {
      ViewBag.ReturnUrl = returnUrl;
      return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
    }

    [ChildActionOnly]
    public ActionResult RemoveExternalLogins()
    {
      ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
      List<ExternalLogin> externalLogins = new List<ExternalLogin>();
      foreach (OAuthAccount account in accounts)
      {
        AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

        externalLogins.Add(new ExternalLogin
        {
          Provider = account.Provider,
          ProviderDisplayName = clientData.DisplayName,
          ProviderUserId = account.ProviderUserId,
        });
      }

      ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
      return PartialView("_RemoveExternalLoginsPartial", externalLogins);
    }

    #region Helpers
    private ActionResult RedirectToLocal(string returnUrl)
    {
      if (Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }
      else
      {
        return RedirectToAction("Index", "Home");
      }
    }

    public enum ManageMessageId
    {
      ChangePasswordSuccess,
      SetPasswordSuccess,
      RemoveLoginSuccess,
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

    private static string ErrorCodeToString(MembershipCreateStatus createStatus)
    {
      // See http://go.microsoft.com/fwlink/?LinkID=177550 for
      // a full list of status codes.
      switch (createStatus)
      {
        case MembershipCreateStatus.DuplicateUserName:
          return "User name already exists. Please enter a different user name.";

        case MembershipCreateStatus.DuplicateEmail:
          return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

        case MembershipCreateStatus.InvalidPassword:
          return "The password provided is invalid. Please enter a valid password value.";

        case MembershipCreateStatus.InvalidEmail:
          return "The e-mail address provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidAnswer:
          return "The password retrieval answer provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidQuestion:
          return "The password retrieval question provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.InvalidUserName:
          return "The user name provided is invalid. Please check the value and try again.";

        case MembershipCreateStatus.ProviderError:
          return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

        case MembershipCreateStatus.UserRejected:
          return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

        default:
          return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
      }
    }
    #endregion
  }
}
