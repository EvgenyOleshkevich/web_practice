using System.Collections.Generic;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class TaskModel
	{
		public TaskData Task { get; set; }

		public class Execute
		{
			public ExecutableData Exe { get; set; }
			public string User { get; set; }
		}
		public List<Execute> Exes { get; set; }

		public List<TestData> Tests { get; set; }

		public int AccessLevel { get; set; }
	}
}
