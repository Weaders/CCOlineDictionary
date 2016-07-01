﻿using Microsoft.AspNet.Identity.Owin;
using OnlineDiary.Models;
using OnlineDiary.Models.People;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace OnlineDiary.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();
        private ApplicationUserManager _userManager = null;

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
            return View(context.Users.ToList());
        }        
        public ActionResult Details(string id)
        {
            DiaryUser dUser = context.Users.Find(id);
            if (dUser == null)
            {
                return HttpNotFound();
            }
            return View(dUser);
        }
        public ActionResult Create()
        {
            var viewModel = new EditUserViewModel();
            return View(viewModel);
        }
        public ActionResult Edit(string id)
        {
            var user = context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var viewModel = new EditUserViewModel(user);
            return View(viewModel);
        }
        public ActionResult Delete(string id)
        {
            DiaryUser dUser = context.Users.Find(id);
            if (dUser == null)
            {
                return HttpNotFound();
            }
            return View(dUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EditUserViewModel dUser)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.CreateAsync(dUser.GetUser(), dUser.Password);
                if (result.Succeeded) {
                    //context.SaveChanges();
                    return RedirectToAction("Details", new { id = dUser.GetUser().Id});
                }
            }
            return HttpNotFound();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditUserViewModel dUser)
        {
            if (ModelState.IsValid)
            {
                //context.Entry(dUser).State = EntityState.Modified;
                var user = await UserManager.FindByIdAsync(dUser.Id);
                if (user != null) {

                    user.FirstName = dUser.FirstName;
                    user.Email = dUser.Email;
                    user.LastName = dUser.LastName;
                    user.PhoneNumber = dUser.PhoneNumber;

                    await UserManager.UpdateAsync(user);

                    if (dUser.Password != null && dUser.Password.Length > 0) {
                        var result = await UserManager.AddPasswordAsync(dUser.Id, dUser.Password);
                        if (!result.Succeeded) {
                            //TODO : Add notif error
                        }
                    }

                    context.SaveChanges();
                    return View(dUser);
                }
            }
            return View(dUser);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DiaryUser dUser = context.Users.Find(id);
            context.Users.Remove(dUser);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            context.Dispose();
            base.Dispose(disposing);
        }
    }
}
