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
using System.Diagnostics;

namespace Web_practice.Utilities
{
	public class Executor
	{
		public Executor(TaskData _task, ExecutableData _exe, string _path_res,
			DataContext _dataContext,
			MyEnvironment _environment)
		{
			dataContext = _dataContext;
			exe = _exe;
			path_res = _path_res + "/";
			environment = _environment;
			env = environment.Env;
			exe.Path_stat = path_res + "statistic.csv";
			stat = new StreamWriter(env + exe.Path_stat);
			task = _task;
			
			StartTests_NoRef();
			StartTests_Ref();
			environment.DeleteFile(exe.Path_exe);
			exe.Path_exe = null;
			dataContext.Attach(exe).State = EntityState.Modified;
			dataContext.SaveChanges();
			stat.Close();
		}


		private readonly DataContext dataContext;
		[Obsolete]
		private readonly IHostingEnvironment appEnvironment;
		private string path_res;
		private string env;
		private StreamWriter stat;
		private ExecutableData exe;
		private TaskData task;
		private MyEnvironment environment;

		private bool DefaultCMP(string path1, string path2)
		{
			return true;
		}

		private bool CMP(string path1, string path2)
		{
			if (task.Path_cmp == null)
				return DefaultCMP(path1, path2);
			return true;
			var process = Process.Start(task.Path_cmp, $"{path1} {path2}");
			process.WaitForExit();
			return "1" == process.StandardOutput.ReadToEnd();
		}

		private void StartTests_Ref()
		{
			var tests = dataContext.Tests.Where(i =>
			i.Task_id == task.Id && i.Path_reference != null).ToArray();
			if (tests.Count() == 0)
				return;

			var times = new int[tests.Count()];
			var is_completes = new bool[tests.Count()];
			stat.WriteLine("test_name; time; result");
			var results = new List<ResultData>();
			for (int i = 0; i < tests.Count(); ++i)
			{
				var test = env + tests[i].Path_test;
				var reference = env + tests[i].Path_reference;
				var res = $"{path_res}{tests[i].Title}.txt";

				var process = Process.Start(env + exe.Path_exe, $"4 {test} {env + res}");
				process.WaitForExit();
				times[i] = process.UserProcessorTime.Milliseconds;
				results.Add(new ResultData()
				{
					Exe_id = exe.Id,
					Test_id = tests[i].Id,
					Path_res = res
				});
				is_completes[i] = CMP(res, reference);
				stat.WriteLine($"{ tests[i].Title}; { times[i]};");
			}
			dataContext.Results.AddRange(results);
		}

		private void StartTests_NoRef()
		{
			var tests = dataContext.Tests.Where(i =>
			i.Task_id == task.Id && i.Path_reference == null).ToArray();
			if (tests.Count() == 0)
				return;

			var times = new int[tests.Count()];
			stat.WriteLine("test_name; time");
			var results = new List<ResultData>();
			for (int i = 0; i < tests.Count(); ++i)
			{
				var test = env + tests[i].Path_test;
				var res = $"{path_res}{tests[i].Title}.txt";

				var process = Process.Start(env + exe.Path_exe, $"4 {test} {env + res}");
				process.WaitForExit();
				times[i] = process.UserProcessorTime.Milliseconds;
				results.Add(new ResultData()
				{
					Exe_id = exe.Id,
					Test_id = tests[i].Id,
					Path_res = res
				});
				stat.WriteLine($"{ tests[i].Title}; { times[i]};");
			}
			dataContext.Results.AddRange(results);
		}
	}
}
