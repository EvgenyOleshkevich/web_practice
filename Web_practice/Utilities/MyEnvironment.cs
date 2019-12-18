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
			IHostingEnvironment appEnvironment)
		{
			dataContext = _dataContext;
			env = appEnvironment.WebRootPath + "/data";
		}

		private readonly DataContext dataContext;
		private readonly string env;

		private class AccessDeleteData
		{
			public TaskAccessData Access { get; set; }
			public TaskData Task { get; set; }
		}

		public async void CreateFileCode_onServer(IFormFile Code, string pathCode)
		{

			// сохраняем файл в папку data/ в каталоге wwwroot
			using (var fileStream = new FileStream(env + pathCode, FileMode.Create))
			{
				await Code.CopyToAsync(fileStream);
			}

		}

		public void DeleteDirictory(string path)
		{
			//Path: /images/game/{nameDir}
			DirectoryInfo dir = new DirectoryInfo(env + path);
			if (dir.Exists)
				DeleteDirictory(dir);
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
			var file = new FileInfo(env + path);
			if (file != null)
			{
				file.Delete();
			}
		}

		private void DeleteStatistics(IEnumerable<StatisticData> statistics)
		{
			var results = (from stat in statistics
						   join res in dataContext.Results on stat.Id equals res.Stat_id
						   select res);
			dataContext.Results.RemoveRange(results);
			dataContext.Statistics.RemoveRange(statistics);
		}

		private void RemoveExecutables(IEnumerable<ExecutableData> executables) // from db
		{
			var statistics = (from exe in executables
							  join stat in dataContext.Statistics on exe.Id equals stat.Exe_id
							  select stat);
			DeleteStatistics(statistics);
			dataContext.Exeсutables.RemoveRange(executables);
		}

		public void Delete(ExecutableData executable)
		{
			var file = new FileInfo(env + executable.Path_exe);
			DeleteDirictory(file.Directory);
			DeleteStatistics(dataContext.Statistics.Where(i => i.Exe_id == executable.Id));
			dataContext.Exeсutables.Remove(executable);
		}

		private void Delete(IEnumerable<ExecutableData> _executables)
		{
			var executables = _executables.ToList();
			foreach (var exe in executables)
			{
				var file = new FileInfo(env + exe.Path_exe);
				DeleteDirictory(file.Directory);
			}
			RemoveExecutables(_executables);
		}

		public void Delete(TaskAccessData access)
		{
			var executables = dataContext.Exeсutables.Where(i => i.Task_id == access.Task_id
			&& i.User_id == access.User_id);
			var task = dataContext.Tasks.Single(i => i.Id == access.Task_id);
			RemoveExecutables(executables);
			string nameDirectory = $"\\data\\{task.User_id}\\{task.Title}\\executables\\{access.User_id}";
			DeleteDirictory(nameDirectory);
			dataContext.TaskAccesses.Remove(access);
		}

		private void Delete(IEnumerable<AccessDeleteData> accesses)
		{
			foreach (var access in accesses)
			{
				string nameDirectory = $"\\data\\{access.Task.User_id}\\{access.Task.Title}\\executables\\{access.Access.User_id}";
				DeleteDirictory(nameDirectory);
			}
			dataContext.TaskAccesses.RemoveRange(accesses.Select(i => i.Access));
		}

		public void Delete(TaskData task)
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

		private void Delete(IEnumerable<TaskData> tasks)
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

			
			RemoveExecutables(executables);
			dataContext.TaskAccesses.RemoveRange(accesses);
			dataContext.Tests.RemoveRange(tests);
			dataContext.Tasks.RemoveRange(tasks);
		}

		public void Delete(UserData user)
		{
			var executables = dataContext.Exeсutables.Where(i => i.User_id == user.Id);
			var tasks = dataContext.Tasks.Where(i => i.User_id == user.Id);
			//var accesses = dataContext.TaskAccesses.Where(i => i.User_id == user.Id);
			var accessesDelete = (from access in dataContext.TaskAccesses.Where(i => i.User_id == user.Id)
							join task in dataContext.Tasks on access.Task_id equals task.Id
							select new AccessDeleteData
							{
								Access = access,
								Task = task
							});
			

			RemoveExecutables(executables);
			Delete(accessesDelete);
			Delete(tasks);
			dataContext.Users.Remove(user);
			string nameDirectory = $"\\data\\{user.Id}";
			DeleteDirictory(nameDirectory);
		}
	}
}
