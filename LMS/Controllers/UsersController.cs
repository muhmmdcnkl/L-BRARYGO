using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using PagedList;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LMS.Models;
using LMS.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace LMS.Controllers
{
    [Authorize(Roles="Manager")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;

        public UsersController()
        {
        }

        public UsersController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        
        public ActionResult Index()
        {
            var users = new List<UserViewModel>();

            foreach (var user in UserManager.Users)
            {
                var u = new UserViewModel
                {
                    ID = user.Id,
                    Name = user.FullName,
                    Email = user.UserName
                };

                users.Add(u);
            }

            foreach (var user in users)
            {
                user.Roles = UserManager.GetRoles(user.ID);
            }

            return View(users);
        }

        
        public ActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                var newUser= new ApplicationUser { FullName = userViewModel.Name, UserName = userViewModel.Email, Email = userViewModel.Email };
                var result = UserManager.Create(newUser, userViewModel.Password);
                if (result.Succeeded) {
                    var res = UserManager.AddToRole(newUser.Id, userViewModel.Roles.ToString());
                    if (res.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    AddErrors(res);
                }
                AddErrors(result);
            }

            return View(userViewModel);
        }

        
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser applicationUser = db.Users.Find(id);

            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            var user = new EditUserViewModel
            {
                ID = applicationUser.Id,
                Name = applicationUser.FullName,
                Email = applicationUser.UserName,
            };

            var Roles = UserManager.GetRoles(applicationUser.Id);

            foreach (var role in Roles)
            {
                if(role == ERoles.Assistant.ToString())
                {
                    user.Roles = ERoles.Assistant;
                }
                if (role == ERoles.Manager.ToString())
                {
                    user.Roles = ERoles.Manager;
                }
            }

            return View(user);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(userViewModel.ID);
                user.FullName = userViewModel.Name;
                user.UserName = userViewModel.Email;
                user.Email = userViewModel.Email;

                var updateResult = UserManager.Update(user);
                if (updateResult.Succeeded)
                {
                    var Roles = UserManager.GetRoles(user.Id);

                    if (!Roles.Contains(userViewModel.Roles.ToString()))
                    {
                        var addRoleResult = UserManager.AddToRole(user.Id, userViewModel.Roles.ToString());
                        if (addRoleResult.Succeeded)
                        {
                            AddErrors(addRoleResult);
                        }
                    }

                    foreach (var role in Roles)
                    {
                        if (role != userViewModel.Roles.ToString())
                        {
                            var removeRoleResult = UserManager.RemoveFromRole(user.Id, role);
                            if (!removeRoleResult.Succeeded)
                            {
                                AddErrors(removeRoleResult);
                            }
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    AddErrors(updateResult);
                }
            }

            return View(userViewModel);
        }

        
        public ActionResult ChangePassword(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser applicationUser = db.Users.Find(id);

            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangeUserPasswordViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = UserManager.FindById(userViewModel.ID);

                var token = UserManager.GeneratePasswordResetToken(user.Id);

                var updateResult = UserManager.ResetPassword(user.Id, token, userViewModel.Password);

                if (updateResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                AddErrors(updateResult);
            }

            return View(userViewModel);
        }

        
        public ActionResult Delete(string id)
        {
            if (id == null || User.Identity.GetUserId() == id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = UserManager.FindById(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var u = new UserViewModel
            {
                ID = user.Id,
                Name = user.FullName,
                Email = user.UserName
            };

            return View(u);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null || User.Identity.GetUserId() == id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = UserManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            UserManager.Delete(user);


            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}
