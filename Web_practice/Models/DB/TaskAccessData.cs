using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class TaskAccessData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("UserData")]
		public int User_id { get; set; }

		[ForeignKey("TaskData")]
		public int Task_id { get; set; }

		[Required]
		public int Level { get; set; }

	}
}
