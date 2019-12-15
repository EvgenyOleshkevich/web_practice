﻿using Microsoft.AspNetCore.Http;
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
			Errors = new List<string>();
		}

		[Required(ErrorMessage = "Введите название задачи")]
		public string Title { get; set; }

		//[Required(ErrorMessage = "Прикрепите исполняемый файл")]
		//public IFormFile Executable { get; set; }


		public IFormFile Comparator { get; set; }

		public List<string> Errors { get; set; }
	}
}
