using System.Collections.Generic;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class ExecutableModel
	{
		public string Statistic { get; set; }
		public ExecutableData Executable { get; set; }

		public IEnumerable<ResultData> Results { get; set; }

		public bool IsCalculated { get; set; }
	}
}


