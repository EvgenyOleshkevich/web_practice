using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class AdditionTestModel
	{
		public AdditionTestModel(string taskIdEncod)
		{
			Errors = new List<string>();
			TaskIdEncod = taskIdEncod;
		}

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

		public string TaskIdEncod { get; set; }
	}
}
