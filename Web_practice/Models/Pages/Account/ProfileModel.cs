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
		public UserData User { get; set; }


		public class TaskUser
		{
			public TaskData Task { get; set; }
			public string User { get; set; }
		}
		public List<TaskUser> Tasks { get; set; }
	}
}
