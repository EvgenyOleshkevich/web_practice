using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Web_practice.Models.Pages.Task;
using Web_practice.Models.DB;
using Web_practice.Models.Pages.Account;
using Web_practice.Models.Pages;

namespace Web_practice.Utilities
{
	public class MyEnvironment
	{
		public MyEnvironment(DataContext _dataContext,
			IHostingEnvironment _appEnvironment)
		{
			dataContext = _dataContext;
			appEnvironment = _appEnvironment;
		}

		private readonly DataContext dataContext;
		[Obsolete]
		private readonly IHostingEnvironment appEnvironment;

		public void DeleteDirictory(string path)
		{
			//Path: /images/game/{nameDir}
			DirectoryInfo dirInfo = new DirectoryInfo(appEnvironment.WebRootPath + path);
			if (dirInfo.Exists)
				DeleteDirictory(dirInfo);
		}

		public void DeleteDirictory(DirectoryInfo dirInfo)
		{
			foreach (var dir in dirInfo.GetDirectories())
				DeleteDirictory(dir);
			foreach (var file in dirInfo.GetFiles())
				file.Delete();
			dirInfo.Delete();
		}
		public void DeleteFile(string path)
		{
			//Path: /images/game/{nameDir}/{nameFile}
			var fileIcon = new FileInfo(appEnvironment.WebRootPath + path);
			if (fileIcon != null)
			{
				fileIcon.Delete();
			}
		}

		public void DeleteExecutable(ExecutableData executable)
		{
			//Path: /images/game/{nameDir}/{nameFile}
			DeleteFile(executable.Path_exe);
		}


		public void DeleteStatistics(IEnumerable<StatisticData> statistics)
		{
			var results = (from stat in statistics
						   join res in dataContext.Results on stat.Id equals res.Stat_id
						   select res);
			dataContext.Results.RemoveRange(results);
			dataContext.Statistics.RemoveRange(statistics);
		}
		public void RemoveExecutables(IEnumerable<ExecutableData> executables) // from db
		{
			var statistics = (from exe in executables
							  join stat in dataContext.Statistics on exe.Id equals stat.Exe_id
							  select stat);
			dataContext.Exeсutables.RemoveRange(executables);
		}

		public void DeleteTask(TaskData task)
		{
			var executables = dataContext.Exeсutables.Where(i => i.Task_id == task.Id);
			var accesses = dataContext.TaskAccesses.Where(i => i.Task_id == task.Id);
			var tests = dataContext.Tests.Where(i => i.Task_id == task.Id);

			string nameDirectory = $"\\data\\{task.User_id}\\{task.Title}";
			DeleteDirictory(nameDirectory);
			RemoveExecutables(executables);
			dataContext.TaskAccesses.RemoveRange(accesses);
			dataContext.Tests.RemoveRange(tests);
			dataContext.Tasks.Remove(task);
		}

		public void DeleteTasks(IEnumerable<TaskData> tasks)
		{
			var executables = (from task in tasks
							   join exe in dataContext.Exeсutables on task.Id equals exe.Task_id
							   select exe);
			var accesses = (from task in tasks
							join access in dataContext.TaskAccesses on task.Id equals access.Task_id
							select access);
			var tests = (from task in tasks
						 join test in dataContext.Tests on task.Id equals test.Task_id
						 select test);
			foreach (var task in tasks)
			{
				string nameDirectory = $"\\data\\{task.User_id}\\{task.Title}";
				DeleteDirictory(nameDirectory);
			}

			RemoveExecutables(executables);
			dataContext.TaskAccesses.RemoveRange(accesses);
			dataContext.Tests.RemoveRange(tests);
			dataContext.Tasks.RemoveRange(tasks);
		}

		public void DeleteUser(UserData user)
		{
			var executables = dataContext.Exeсutables.Where(i => i.User_id == user.Id);
			var tasks = dataContext.Exeсutables.Where(i => i.User_id == user.Id);
			var accesses = dataContext.TaskAccesses.Where(i => i.User_id == user.Id);

			DeleteExecutables(executables);
			dataContext.TaskAccesses.RemoveRange(accesses);
			DeleteTasks(tasks);
		}
	}
}
