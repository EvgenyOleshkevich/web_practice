using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class TaskData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("UserData")]
		public int User_id { get; set; }

		[MaxLength(50)]
		[Required]
		public string Title { get; set; }

		[MaxLength(255)]
		public string Path_cmp { get; set; }
	}
}
