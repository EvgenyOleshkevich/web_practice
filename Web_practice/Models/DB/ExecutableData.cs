using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class ExecutableData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("UserData")]
		public int User_id { get; set; }

		[ForeignKey("TaskData")]
		public int Task_id { get; set; }

		[MaxLength(100)]
		[Required]
		public string Title { get; set; }

		[MaxLength(255)]
		[Required]
		public string Path_exe { get; set; }

		[MaxLength(255)]
		[Required]
		public string Path_cmp { get; set; }

		[Required]
		public bool Iscalculating { get; set; }
	}
}
