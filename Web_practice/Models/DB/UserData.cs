using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class UserData
	{
		[Key]
		public int Id { get; set; }

		[MaxLength(100)]
		[Required]
		public string Login { get; set; }

		[MaxLength(100)]
		[Required]
		public string Password { get; set; }

		[MaxLength(100)]
		[Required]
		public string Email { get; set; }

		[Required]
		public string PathAvatar { get; set; }

		public IList<TaskData> Executables { get; set; }
	}
}
