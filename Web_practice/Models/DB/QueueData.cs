using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class QueueData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("ExecutableData")]
		public int Exe_id { get; set; }
	}
}
