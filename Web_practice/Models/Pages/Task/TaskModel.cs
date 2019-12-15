using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class TaskModel
	{
		public TaskData Task { get; set; }

		public List<ExecutableData> Exes { get; set; }

		public List<TestData> Tests { get; set; }
	}
}
