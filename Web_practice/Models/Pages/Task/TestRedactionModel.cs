using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class TestRedactionModel
	{
		public TestRedactionModel(TestData test)
		{
			Test = test;
			Messages = new List<string>();
		}

		public TestData Test { get; set; }
		public string Title { get; set; }

		public IFormFile Test_path { get; set; }

		public IFormFile Reference_path { get; set; }

		public List<string> Messages { get; set; }
	}
}
