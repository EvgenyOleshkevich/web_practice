using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web_practice.Models.Pages.Task
{
	public class AdditionExecutableModel
	{
		public AdditionExecutableModel(string taskIdEncod)
		{
			Errors = new List<string>();
			TaskIdEncod = taskIdEncod;
		}

		public AdditionExecutableModel()
		{
			Errors = new List<string>();
		}

		[Required(ErrorMessage = "Введите название задачи")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Прикрепите исполняемый файл")]
		public IFormFile Executable { get; set; }

		[MaxLength(255)]
		[Required(ErrorMessage = "Введите строку интерполяции входных данных")]
		public string InterpolationString { get; set; }

		public List<string> Errors { get; set; }
		public string TaskIdEncod { get; set; }
	}
}
