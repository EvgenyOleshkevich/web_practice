using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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


