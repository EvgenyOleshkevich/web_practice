﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_practice.Models.Pages.Task
{
	public class AccessChangeModel
	{

		public AccessChangeModel(string accessIdEncode)
		{
			AccessIdEncode = accessIdEncode;
			Errors = new List<string>();
		}
		public AccessChangeModel()
		{
			Errors = new List<string>();
		}

		[Required(ErrorMessage = "Введите уровень доступа")]
		public int Level { get; set; }

		public List<string> Errors { get; set; }

		public string AccessIdEncode { get; set; }
	}
}
