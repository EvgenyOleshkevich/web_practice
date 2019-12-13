using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Account
{
	public class ProfileModel
	{
		public ProfileModel()
		{
			Errors = new List<string>();
			Gen_task(5);
			
		}

		public List<string> Errors { get; set; }


		public UserData User { get; set; }

		public List<ExecutableData> Tasks { get; set; }

		private void Gen_task(int count)
		{
			Tasks = new List<ExecutableData>();
			for (int i = 0; i < count; ++i)
			{
				var task = new ExecutableData()
				{
					Title = "program " + i.ToString(),
					Id = i
				};
				Tasks.Add(task);
			}
		}
	}
}
