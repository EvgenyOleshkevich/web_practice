﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Account
{
	public class ProfileSettingsModel
	{
		public ProfileSettingsModel()
		{
			Errors = new List<string>();
			userInfo = new UserInfo();
		}

		public ProfileSettingsModel(UserData user)
		{
			User = user;
			Errors = new List<string>();
			userInfo = new UserInfo()
			{
				Email = user.Email,
				Login = user.Login,
				Password = user.Password
			};
		}

		public UserData User { get; set; }
		public List<string> Errors { get; set; }

		public class UserInfo
		{

			[Required(ErrorMessage = "Не указан логин")]
			public string Login { get; set; }

			[Required(ErrorMessage = "Не указан Email")]
			[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)"
			+ @"@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", ErrorMessage = "Указан не корректный Email")]
			public string Email { get; set; }

			//[Required(ErrorMessage = "Не указан пароль")]
			[DataType(DataType.Password)]
			public string Password { get; set; }

			[DataType(DataType.Password)]
			[Compare("Password", ErrorMessage = "Пароль введен неверно")]
			public string ConfirmPassword { get; set; }
		}

		[BindProperty]
		public UserInfo userInfo { get; set; }
	}
}
