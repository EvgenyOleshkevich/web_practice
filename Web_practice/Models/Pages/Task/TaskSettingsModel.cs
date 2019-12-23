using System.Collections.Generic;
using Web_practice.Models.DB;

namespace Web_practice.Models.Pages.Task
{
	public class TaskSettingsModel
	{

		public class Follower
		{
			public TaskAccessData Access { get; set; }
			public string User { get; set; }
		}
		public List<Follower> Followers { get; set; }
	}
}
