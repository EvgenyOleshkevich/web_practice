using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web_practice.Models.Pages
{
	public class Index
	{
		//[Required(ErrorMessage = "Не указан логин")]
		public string Login { get; set; }

		//[Required(ErrorMessage = "Не указан пароль")]
		//[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
