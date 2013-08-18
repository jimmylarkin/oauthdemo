using JobDone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JobDone.Controllers
{
  public class ProfileController : Controller
  {
    public ActionResult Index()
    {
      using (DataContext db = new DataContext())
      {
        UserProfile user = db.UserProfiles.FirstOrDefault(e => e.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
        var model = new ProfileModel { UserName = user.UserName };
        return View(model);
      }
    }
  }
}
