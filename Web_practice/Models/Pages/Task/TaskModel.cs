using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class TaskModel
	{
		public TaskModel()
		{
			Errors = new List<string>();
			Tests = new List<TestData>();
			Exe = new ExecutableData()
			{
				Title = "program " + (0).ToString(),
				Id = 0
			};
			GenerateTest(5);
		}

		public TaskModel(int id)
		{
			Errors = new List<string>();
			Tests = new List<TestData>();
			Exe = new ExecutableData()
			{
				Title = "program " + id.ToString(),
				Id = id
			};
			GenerateTest(5);
		}

		public List<string> Errors { get; set; }


		public ExecutableData Exe { get; set; }

		public List<TestData> Tests { get; set; }

		private void GenerateTest(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				var test = new TestData()
				{
					Title = "program " + i.ToString(),
					Id = i
				};
				Tests.Add(test);
			}
		}
	}
}
