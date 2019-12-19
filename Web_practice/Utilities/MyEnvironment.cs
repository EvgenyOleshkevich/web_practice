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
		public static MyEnvironment GetInstance(DataContext dataContext)
		{
			instance.dataContext = dataContext;
			return instance;
		}

		public static void Init(DataContext dataContext, string env)
		{
			instance = new MyEnvironment(dataContext, env);
		}

		private MyEnvironment(DataContext _dataContext,
			IHostingEnvironment appEnvironment)
		{
			dataContext = _dataContext;
			Env = appEnvironment.WebRootPath + "/data/";
		}

		private MyEnvironment(DataContext _dataContext,
			string env)
		{
			dataContext = _dataContext;
			Env = env;
		}


		private static MyEnvironment instance;
		private DataContext dataContext;
		public string Env { get; }

		private class AccessDeleteData
		{
			public TaskAccessData Access { get; set; }
			public TaskData Task { get; set; }
		}

		public void CreateDirectory(string path)
		{
			Directory.CreateDirectory(Env + path);
		}

		public async void CreateFileCode(IFormFile Code, string pathCode)
		{
			using (var fileStream = new FileStream(Env + pathCode, FileMode.Create))
			{
				await Code.CopyToAsync(fileStream);
			}

		}

		public void DeleteDirectory(string path)
		{
			DirectoryInfo dir = new DirectoryInfo(Env + path);
			if (dir.Exists)
				DeleteDirectory(dir);
		}

		public void DeleteDirectory(DirectoryInfo dirInfo)
		{
			foreach (var dir in dirInfo.GetDirectories())
				DeleteDirectory(dir);
			foreach (var file in dirInfo.GetFiles())
				file.Delete();
			dirInfo.Delete();
		}

		public void DeleteFile(string path)
		{
			var file = new FileInfo(Env + path);
			if (file != null)
				file.Delete();
		}

		private void RemoveExecutables(IEnumerable<ExecutableData> executables) // from db
		{
			var results = (from exe in executables
							  join res in dataContext.Results on exe.Id equals res.Exe_id
							  select res);
			dataContext.Results.RemoveRange(results);
			dataContext.Exeсutables.RemoveRange(executables);
		}

		public async Task Delete(ExecutableData executable)
		{
			FileInfo file;

			if (Executor.GetInstance(dataContext).Token != null)
			{
				Executor.GetInstance(dataContext).Token.Cancel();
				await Executor.GetInstance(dataContext).Execution;
				Executor.GetInstance(dataContext).Token.Dispose();
				Executor.GetInstance(dataContext).Token = null;
			}
			if (executable.Path_exe != null)
				file = new FileInfo(Env + executable.Path_exe);
			else
				file = new FileInfo(Env + executable.Path_stat);
			DeleteDirectory(file.Directory);
			dataContext.Results.RemoveRange(dataContext.Results.Where(i => i.Exe_id == executable.Id));
			dataContext.Exeсutables.Remove(executable);
		}

		private void Delete(IEnumerable<ExecutableData> _executables)
		{
			var executables = _executables.ToList();
			foreach (var exe in executables)
			{
				var file = new FileInfo(Env + exe.Path_exe);
				DeleteDirectory(file.Directory);
			}
			RemoveExecutables(_executables);
		}

		public void Delete(TaskAccessData access)
		{
			var executables = dataContext.Exeсutables.Where(i => i.Task_id == access.Task_id
			&& i.User_id == access.User_id);
			var task = dataContext.Tasks.Single(i => i.Id == access.Task_id);
			RemoveExecutables(executables);
			string nameDirectory = $"{task.User_id}\\{task.Title}\\executables\\{access.User_id}";
			DeleteDirectory(nameDirectory);
			dataContext.TaskAccesses.Remove(access);
		}

		private void Delete(IEnumerable<AccessDeleteData> accesses)
		{
			foreach (var access in accesses)
			{
				string nameDirectory = $"\\data\\{access.Task.User_id}\\{access.Task.Title}\\executables\\{access.Access.User_id}";
				DeleteDirectory(nameDirectory);
			}
			dataContext.TaskAccesses.RemoveRange(accesses.Select(i => i.Access));
		}

		public void Delete(TaskData task)
		{
			var executables = dataContext.Exeсutables.Where(i => i.Task_id == task.Id);
			var accesses = dataContext.TaskAccesses.Where(i => i.Task_id == task.Id);
			var tests = dataContext.Tests.Where(i => i.Task_id == task.Id);

			DeleteDirectory($"{task.User_id}\\{task.Title}");
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
			DeleteDirectory(user.Id.ToString());
		}
	}
}
