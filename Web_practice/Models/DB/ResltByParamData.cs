using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class ResltByParamData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("StatisticData")]
		public int Stat_id { get; set; }


		[ForeignKey("TestData")]
		public int Test_id { get; set; }

		[MaxLength(255)]
		[Required]
		public string Path_res { get; set; }

		[Required]
		public int Param { get; set; }
	}
}
