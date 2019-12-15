using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web_practice.Models.Pages.Task
{
	public class ExecutableModel
	{
		public ExecutableModel()
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


