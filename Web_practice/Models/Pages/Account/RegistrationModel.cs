using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web_practice.Models.Pages.Account
{
	public class RegistrationModel
	{
		public RegistrationModel()
		{
			Errors = new List<string>();
			userInfo = new UserInfo();
		}

		public List<string> Errors { get; set; }

		public class UserInfo
		{

			//[Required(ErrorMessage = "Не указан логин")]
			public string Login { get; set; }

			//[Required(ErrorMessage = "Не указан Email")]
			//[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)"
			//+ @"@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", ErrorMessage = "Указан не корректный Email")]
			public string Email { get; set; }

			//[Required(ErrorMessage = "Не указан пароль")]
			//[DataType(DataType.Password)]
			public string Password { get; set; }

			//[DataType(DataType.Password)]
			//[Compare("Password", ErrorMessage = "Пароль введен неверно")]
			public string ConfirmPassword { get; set; }
		}

		//[BindProperty]
		public UserInfo userInfo { get; set; }
	}
}
