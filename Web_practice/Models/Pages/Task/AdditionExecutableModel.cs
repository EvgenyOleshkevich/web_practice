using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_practice.Models.Pages.Task
{
	public class AdditionExecutableModel
	{
		public AdditionExecutableModel()
		{
			Errors = new List<string>();
		}

		[Required(ErrorMessage = "Введите название задачи")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Прикрепите исполняемый файл")]
		public IFormFile Executable { get; set; }

		public List<string> Errors { get; set; }
	}
}
