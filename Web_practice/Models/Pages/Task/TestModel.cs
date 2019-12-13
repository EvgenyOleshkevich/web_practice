using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;


namespace Web_practice.Models.Pages.Task
{
	public class TestModel
	{
		public TestModel()
		{
			Messages = new List<string>();
		}

		//[Required(ErrorMessage = "Введите название задачи")]
		public string Title { get; set; }

		//[Required(ErrorMessage = "Прикрепите исполняемый файл")]
		public IFormFile Test_path { get; set; }

		public IFormFile Reference_path { get; set; }

		public List<string> Messages { get; set; }
	}
}
