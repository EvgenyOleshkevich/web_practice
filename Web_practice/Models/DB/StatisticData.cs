using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class StatisticData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("Executable")]
		public int Exe_id { get; set; }

		[MaxLength(255)]
		[Required]
		public string Path_stat { get; set; }

		public int Param_from { get; set; }
		public int Param_to { get; set; }
	}
}
