﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_practice.Models.Pages.Account;
using Web_practice.Models.Pages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Web_practice.Models.DB;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Web_practice.Controllers
{
	public class AccountController : Controller
	{
		private readonly DataContext dataContext;
		[Obsolete]
		private readonly IHostingEnvironment appEnvironment;

		public AccountController(DataContext _dataContext,
			IHostingEnvironment _appEnvironment)
		{
			dataContext = _dataContext;
			appEnvironment = _appEnvironment;
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

					var userId = dataContext.Users.FirstOrDefault(i => i.Login == user.Login).Id;

					await Authorize(userId.ToString()); // авторизация

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
					new Claim(ClaimsIdentity.DefaultNameClaimType, DB_id)
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
				var user = dataContext.Users.Single(i => i.Id == userId);
				return View(new ProfileModel()
				{
					User = user,
					Tasks = dataContext.Tasks.Where(i => i.User_id == userId).ToList()
				});
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
			dataContext.Users.Remove(user);
			dataContext.SaveChanges();
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return Redirect("~/Home/Index");
		}
		#endregion
	}
}