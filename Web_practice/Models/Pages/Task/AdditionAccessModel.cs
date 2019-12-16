using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class AdditionAccessModel
	{
		public AdditionAccessModel()
		{
			Errors = new List<string>();
		}

		[Required(ErrorMessage = "Введите логин")]
		public string Login { get; set; }

		[Required(ErrorMessage = "Введите уровень доступа")]
		public int Level { get; set; }

		public List<string> Errors { get; set; }
	}
}
