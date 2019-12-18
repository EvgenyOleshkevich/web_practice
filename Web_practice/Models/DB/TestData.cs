using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class TestData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("TaskData")]
		public int Task_id { get; set; }


		[MaxLength(50)]
		[Required]
		public string Title { get; set; }

		[MaxLength(255)]
		[Required]
		public string Path_test { get; set; }

		[MaxLength(255)]
		public string Path_reference { get; set; }
	}
}
