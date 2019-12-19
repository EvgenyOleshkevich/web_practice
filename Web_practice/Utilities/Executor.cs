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
using System.Threading;

namespace Web_practice.Utilities
{
	public class Executor
	{
		public static Executor GetInstance(DataContext dataContext)
		{
			instance.dataContext = dataContext;
			return instance;
		}

		public static void Init(DataContext dataContext)
		{
			instance = new Executor(dataContext);
		}

		public void StartTests(TaskData _task, ExecutableData _exe, string _path_res)
		{
			exe = _exe;
			path_res = _path_res + "/";
			
			task = _task;
			//StartTests();
			tests1 = dataContext.Tests.Where(i =>
			i.Task_id == task.Id && i.Path_reference == null).ToArray();
			tests2 = dataContext.Tests.Where(i =>
			i.Task_id == task.Id && i.Path_reference == null).ToArray();

			Token = new CancellationTokenSource();
			C_token = Token.Token;

			Execution = Task.Run(() =>
			{
				// Were we already canceled?
				C_token.ThrowIfCancellationRequested();
				StartTests();
			}, Token.Token);
			//q.Start();
		}

		public void StartTests()
		{
			exe.Path_stat = path_res + "statistic.csv";
			stat = new StreamWriter(env + exe.Path_stat);
			StartTests_NoRef();
			if (C_token.IsCancellationRequested)
				//C_token.ThrowIfCancellationRequested();
				return;
			StartTests_Ref();
			MyEnvironment.GetInstance(dataContext).DeleteFile(exe.Path_exe);
			exe.Path_exe = null;
			dataContext.Attach(exe).State = EntityState.Modified;
			dataContext.SaveChanges();
			stat.Close();
		}

		private Executor(TaskData _task, ExecutableData _exe, string _path_res,
			DataContext _dataContext,
			MyEnvironment environment)
		{
			dataContext = _dataContext;
			exe = _exe;
			path_res = _path_res + "/";
			env = environment.Env;
			exe.Path_stat = path_res + "statistic.csv";
			stat = new StreamWriter(env + exe.Path_stat);
			task = _task;
		}

		private Executor(DataContext _dataContext)
		{
			dataContext = _dataContext;
			env = MyEnvironment.GetInstance(dataContext).Env;
		}

		static private Executor instance;
		private DataContext dataContext;
		[Obsolete]
		private string path_res;
		private string env;
		private StreamWriter stat;
		private ExecutableData exe;
		private TaskData task;
		private TestData[] tests1;
		private TestData[] tests2;
		public Task Execution { get; private set; }
		public CancellationToken C_token { get; private set; }
		public CancellationTokenSource Token { get; set; }

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
			if (tests2.Count() == 0)
				return;

			var times = new int[tests2.Count()];
			var is_completes = new bool[tests2.Count()];
			stat.WriteLine("test_name; time; result");
			var results = new List<ResultData>();
			for (int i = 0; i < tests2.Count(); ++i)
			{
				var test = env + tests2[i].Path_test;
				var reference = env + tests2[i].Path_reference;
				var res = $"{path_res}{tests2[i].Title}.txt";

				var process = Process.Start(env + exe.Path_exe, $"1 {test} {env + res}");
				process.WaitForExit();
				times[i] = process.UserProcessorTime.Milliseconds;
				results.Add(new ResultData()
				{
					Exe_id = exe.Id,
					Test_id = tests2[i].Id,
					Path_res = res
				});
				is_completes[i] = CMP(res, reference);
				stat.WriteLine($"{ tests2[i].Title}; { times[i]};");
			}
			dataContext.Results.AddRange(results);
		}

		private void StartTests_NoRef()
		{
			if (tests1.Count() == 0)
				return;

			var times = new int[tests1.Count()];
			stat.WriteLine("test_name; time");
			var results = new List<ResultData>();
			for (int i = 0; i < tests1.Count(); ++i)
			{
				var test = env + tests1[i].Path_test;
				var res = $"{path_res}{tests1[i].Title}.txt";

				var process = Process.Start(env + exe.Path_exe, $"1 {test} {env + res}");
				//process.WaitForExit();
				while (!process.HasExited)
				{
					if (C_token.IsCancellationRequested)
					{
						process.Kill();
						//C_token.ThrowIfCancellationRequested();
						stat.Close();
						return;
					}
				}
				times[i] = process.UserProcessorTime.Milliseconds;
				results.Add(new ResultData()
				{
					Exe_id = exe.Id,
					Test_id = tests1[i].Id,
					Path_res = res
				});
				stat.WriteLine($"{ tests1[i].Title}; { times[i]};");
			}
			dataContext.Results.AddRange(results);
		}

		
	}
}
