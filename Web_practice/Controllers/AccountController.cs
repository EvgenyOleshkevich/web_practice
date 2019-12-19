using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using Web_practice.Models.DB;
using Web_practice.Utilities;
using Web_practice.Models.Pages.Account;
using Web_practice.Models.Pages;
using System.IO;

namespace Web_practice.Controllers
{
	public class AccountController : Controller
	{
		private readonly DataContext dataContext;
		private readonly MyEnvironment environment;

		public AccountController(DataContext _dataContext,
			IDataProtectionProvider provider,
			IHostingEnvironment appEnvironment)
		{
			ProtectData.GetInstance().Initialize(provider);
			dataContext = _dataContext;
			//environment = new MyEnvironment(dataContext, appEnvironment);
			environment = MyEnvironment.GetInstance(dataContext);
		}


		#region Authorization and Registration

		[HttpGet]
		public IActionResult Registration()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new RegistrationModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Registration(RegistrationModel model)
		{
			if (ModelState.IsValid)
			{
				if (dataContext.Users.FirstOrDefault(i => i.Login == model.userInfo.Login) != null)
				{
					model.Errors.Add("Пользователь с таким логином уже существует");
					ModelState.AddModelError("", "Пользователь с таким логином уже существует");
				}

				if (dataContext.Users.FirstOrDefault(i => i.Email == model.userInfo.Email) != null)
				{
					model.Errors.Add("Пользователь с таким адресом эл.почты уже существует");
					ModelState.AddModelError("", "Пользователь с таким адресом эл.почты уже существует");
				}

				if (!ModelState.IsValid)
				{
					return View(model);
				}
				else
				{
					var user = new UserData
					{
						Login = model.userInfo.Login,
						Email = model.userInfo.Email,
						Password = model.userInfo.Password
					};

					try
					{
						dataContext.Users.Add(user);
						dataContext.SaveChanges();
						
					}
					catch (Exception ex)
					{
						throw ex;
					}

					var userId = dataContext.Users.FirstOrDefault(i => i.Login == user.Login).Id.ToString();

					//Directory.CreateDirectory($"./wwwroot/data/{userId}");
					environment.CreateDirectory(userId);
					await Authorize(userId); // авторизация

					return RedirectToAction("Profile", "Account");
				}
			}
			return View(model);
		}



		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Authorization(IndexModel model)
		{
			if (ModelState.IsValid)
			{
				var user = dataContext.Users.FirstOrDefault(i => i.Login == model.Login && i.Password == model.Password);
				if (user != null)
				{
					await Authorize(user.Id.ToString()); // авторизация
					return RedirectToAction("Profile", "Account");
				}

				ViewBag.Error = "Некорректные логин и(или) пароль";
				ModelState.AddModelError("", "Некорректные логин и(или) пароль");
			}
			return Redirect("~/Home/Index");
		}

		private async Task Authorize(string DB_id)
		{
			var claims = new List<Claim>
				{
					new Claim(ClaimsIdentity.DefaultNameClaimType, DB_id),
				};

			ClaimsIdentity id = new ClaimsIdentity(
				claims,
				"ApplicationCoockie",
				ClaimsIdentity.DefaultNameClaimType,
				ClaimsIdentity.DefaultRoleClaimType
				);

			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect("~/Home/Index");
		}
		#endregion

		#region Profile

		[Authorize]
		[HttpGet]
		public IActionResult Profile()
		{

			try
			{
				var userId = Int32.Parse(HttpContext.User.Identity.Name);
				//var q = HttpContext.User.Claims.Last().Value;
				var user = dataContext.Users.Single(i => i.Id == userId);
				var model = new ProfileModel() { User = user };

				var tasks = (from access in dataContext.TaskAccesses.Where(i => i.User_id == userId)
							   join task in dataContext.Tasks on access.Task_id equals task.Id
							   select new ProfileModel.TaskUser
							   {
								   User = dataContext.Users.FirstOrDefault(i => i.Id == task.User_id).Login,
								   Task = task
							   }).ToList();

				model.Tasks = (from task in dataContext.Tasks.Where(i => i.User_id == userId)

							   select new ProfileModel.TaskUser
							   {
								   User = user.Login,
								   Task = task
							   }).ToList();
				model.Tasks.AddRange(tasks);
				return View(model);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		[HttpPost]
		public async Task<IActionResult> Profile(ProfileModel model)
		{
			return View(new ProfileModel());
		}

		[Authorize]
		[HttpGet]
		public IActionResult ProfileSettings()
		{
			var userId = Int32.Parse(HttpContext.User.Identity.Name);
			var user = dataContext.Users.Single(i => i.Id == userId);
			if (user == null)
				throw new Exception("user is invalid");
			return View(new ProfileSettingsModel(user));
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> ProfileSettings(ProfileSettingsModel model)
		{
			if (ModelState.IsValid)
			{
				var userId = Int32.Parse(HttpContext.User.Identity.Name);
				var user = dataContext.Users.Single(i => i.Id == userId);

				if (dataContext.Users.FirstOrDefault(i => i.Login == model.userInfo.Login) != null
					&& user.Login != model.userInfo.Login)
				{
					model.Errors.Add("Пользователь с таким логином уже существует");
					ModelState.AddModelError("", "Пользователь с таким логином уже существует");
				}

				if (dataContext.Users.FirstOrDefault(i => i.Email == model.userInfo.Email) != null
					&& user.Email != model.userInfo.Email)
				{
					model.Errors.Add("Пользователь с таким адресом эл.почты уже существует");
					ModelState.AddModelError("", "Пользователь с таким адресом эл.почты уже существует");
				}

				user.Login = model.userInfo.Login;
				user.Email = model.userInfo.Email;
				if (model.userInfo.Password != null)
					user.Password = model.userInfo.Password;

				try
				{
					dataContext.Attach(user).State = EntityState.Modified;
					dataContext.SaveChanges();
				}
				catch (Exception ex)
				{
					// user will see error page
					throw ex;
				}

				return Redirect("~/Account/Profile");
			}
			return View(model);
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> ProfileDelete(ProfileSettingsModel model)
		{

			var userId = Int32.Parse(HttpContext.User.Identity.Name);
			var user = dataContext.Users.Single(i => i.Id == userId);
			environment.Delete(user);
			dataContext.SaveChanges();
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return Redirect("~/Home/Index");
		}
		#endregion
	}
}