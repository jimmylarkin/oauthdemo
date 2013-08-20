﻿using JobDone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace JobDone.Controllers
{
  public class LoginController : Controller
  {
    [AllowAnonymous]
    public ActionResult Index()
    {
      return View(new LoginModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public ActionResult Index(LoginModel model, string returnUrl)
    {
      if (ModelState.IsValid)
      {
        using (DataContext db = new DataContext())
        {
          UserProfile user = db.UserProfiles.FirstOrDefault(e => e.UserName.Equals(model.UserName, StringComparison.OrdinalIgnoreCase));
          if (user != null && user.Password == model.Password)
          {
            FormsAuthentication.SetAuthCookie(model.UserName, false);
            if (Url.IsLocalUrl(returnUrl))
            {
              return Redirect(returnUrl);
            }
            else
            {
              return RedirectToAction("Index", "Home");
            }
          }
        }
      }
      ModelState.AddModelError("", "The user name or password provided is incorrect.");
      return View(model);
    }

    public ActionResult Logoff()
    {
      FormsAuthentication.SignOut();
      return RedirectToAction("Index", "Home");
    }
  }
}