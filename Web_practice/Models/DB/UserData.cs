using System.ComponentModel.DataAnnotations;

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
	}
}
