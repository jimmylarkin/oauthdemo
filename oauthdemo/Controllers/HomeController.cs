using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using oauthdemo.Models;

namespace oauthdemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            HomeModel model = new HomeModel();
            if (User.Identity.IsAuthenticated)
            {
                using (UsersContext db = new UsersContext())
                {
                    var userTasks = db.Tasks.Include("User").Where(t => t.User.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderByDescending(t => t.Id).ToList();
                    model.Tasks = userTasks.Where(t => !t.Completed.HasValue).ToList();
                    model.ToDoCount = model.Tasks.Count;
                    model.CompletedCount = userTasks.Where(t => t.Completed.HasValue).Count();
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(HomeModel model)
        {
            if (ModelState.IsValid)
            {
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(e => e.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
                    db.Tasks.Add(new TaskEntry() { Description = model.TaskDescription, Estimation = model.TaskEstimation, User = user });
                    db.SaveChanges();
                    var userTasks = db.Tasks.Include("User").Where(t => t.User.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase)).OrderByDescending(t => t.Id).ToList();
                    model.Tasks = userTasks.Where(t => !t.Completed.HasValue).ToList();
                    model.ToDoCount = model.Tasks.Count;
                    model.CompletedCount = userTasks.Where(t => t.Completed.HasValue).Count();
                }
            }
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
