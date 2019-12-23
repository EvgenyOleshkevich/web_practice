using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_practice.Models.Pages.Task
{
	public class AdditionTestModel
	{
		public AdditionTestModel()
		{
			Errors = new List<string>();
		}

		[Required(ErrorMessage = "Введите название теста")]
		public string Title { get; set; }


		[Required(ErrorMessage = "Прикрепите тестовый")]
		public IFormFile Test_path { get; set; }

		public IFormFile Reference_path { get; set; }

		public List<string> Errors { get; set; }
	}
}
