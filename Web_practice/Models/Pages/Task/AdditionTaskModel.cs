using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class AdditionTaskModel
	{
		public AdditionTaskModel()
		{
			Messages = new List<string>();
			Iscreated = false;
		}

		public AdditionTaskModel(TaskData task)
		{
			Messages = new List<string>();
			Title = task.Title;
			Iscreated = true;
		}

		public AdditionTaskModel(ExecutableData task)
		{
			Messages = new List<string>();
			Title = task.Title;
			Iscreated = true;
		}

		//[Required(ErrorMessage = "Введите название задачи")]
		public string Title { get; set; }

		//[Required(ErrorMessage = "Прикрепите исполняемый файл")]
		public IFormFile Executable { get; set; }


		public IFormFile Comparator { get; set; }

		public List<string> Messages { get; set; }

		public bool Iscreated { get; set; }
	}
}
