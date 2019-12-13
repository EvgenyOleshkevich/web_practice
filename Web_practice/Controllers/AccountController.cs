using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_practice.Models.Pages.Account;
using Web_practice.Models.Pages;

namespace Web_practice.Controllers
{
    public class AccountController : Controller
    {

		#region Authorization and Registration

		[HttpGet]
		public IActionResult Registration()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new RegistrationModel());
		}

		[HttpPost]
		public async Task<IActionResult> Registration(RegistrationModel model)
		{
			return Redirect("Profile");
		}
		#endregion

		#region Profile

		[HttpGet]
		public IActionResult Profile()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new ProfileModel());
		}

		[HttpPost]
		public async Task<IActionResult> Profile(ProfileModel model)
		{
			return View(new ProfileModel());
		}

		#endregion


		// GET: Index
		public ActionResult Index()
        {
            return View();
        }

        // GET: Index/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Index/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Index/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Index/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Index/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Index/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Index/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}