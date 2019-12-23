using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_practice.Models.DB
{
	public class ResultData
	{
		[Key]
		public int Id { get; set; }

		[ForeignKey("ExecutableData")]
		public int Exe_id { get; set; }


		[ForeignKey("TestData")]
		public int Test_id { get; set; }

		[MaxLength(255)]
		[Required]
		public string Path_res { get; set; }
	}
}
