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
		public Executor(TaskData _task, ExecutableData _exe, string path_res,
			DataContext _dataContext,
			IHostingEnvironment _appEnvironment)
		{
			dataContext = _dataContext;
			appEnvironment = _appEnvironment;
			exe = _exe;
			path_out = appEnvironment.WebRootPath + path_res + "/";
			stat = new StreamWriter(path_out + "statistic.csv");
			task = _task;
			statistic = new StatisticData()
			{
				Exe_id = exe.Id,
				Path_stat = path_out + "statistic.csv"
			};
			dataContext.Statistics.Add(statistic);
			dataContext.SaveChanges();
			statistic.Id = dataContext.Statistics.Single(i => i.Exe_id == exe.Id).Id;
		}


		private readonly DataContext dataContext;
		[Obsolete]
		private readonly IHostingEnvironment appEnvironment;
		private string path_out;
		private StreamWriter stat;
		private ExecutableData exe;
		private TaskData task;
		private StatisticData statistic;

		private bool DefaultCMP(string path1, string path2)
		{
			return true;
		}

		private bool CMP(string path1, string path2)
		{
			if (task.Path_cmp == null)
				return DefaultCMP(path1, path2);
			var process = Process.Start(task.Path_cmp, $"{path1} {path2}");
			process.WaitForExit();
			return "1" == process.StandardOutput.ReadToEnd();
		}

		private void StartTests_Ref()
		{
			string env = appEnvironment.WebRootPath;
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
				var res = $"{path_out}{tests[i].Title}.txt";

				var process = Process.Start(env + exe.Path_exe, $"4 {test} {res}");
				process.WaitForExit();
				times[i] = process.UserProcessorTime.Milliseconds;
				results.Add(new ResultData()
				{
					Stat_id = statistic.Id,
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
			string env = appEnvironment.WebRootPath;
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
				var res = $"{path_out}{tests[i].Title}.txt";

				var process = Process.Start(env + exe.Path_exe, $"4 {test} {res}");
				process.WaitForExit();
				times[i] = process.UserProcessorTime.Milliseconds;
				results.Add(new ResultData()
				{
					Stat_id = statistic.Id,
					Test_id = tests[i].Id,
					Path_res = res
				});
				stat.WriteLine($"{ tests[i].Title}; { times[i]};");
			}
			dataContext.Results.AddRange(results);
		}

		public void StartTests()
		{
			StartTests_NoRef();
			dataContext.SaveChanges();
			stat.Close();
		}
	}
}
