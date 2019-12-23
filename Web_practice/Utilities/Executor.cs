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
using System.ComponentModel;

namespace Web_practice.Utilities
{
	public class Executor
	{
		public static Executor GetInstance()
		{
			if (instance == null)
				instance = new Executor();
			return instance;
		}

		public void AddExecutable(TaskData _task, ExecutableData _exe, string _path_res, DataContext dataContext)
		{
			dataContext.Exeсutables.Add(_exe);
			dataContext.SaveChanges();
			var pr = new Program
			{
				path_res = _path_res + "/",
				tests_ref = dataContext.Tests.Where(i =>
					i.Task_id == _task.Id && i.Path_reference != null).ToArray(),
				tests = dataContext.Tests.Where(i =>
					i.Task_id == _task.Id && i.Path_reference == null).ToArray(),
				exe = _exe,
				path_cmp = _task.Path_cmp
			};
			var stat = new StreamWriter(env + _exe.Path_stat);
			stat.WriteLine(0);
			stat.Close();
			var results = new List<ResultData>();
			
			foreach (var test in pr.tests)
			{
				results.Add(new ResultData()
				{
					Exe_id = pr.exe.Id,
					Test_id = test.Id,
					Path_res = $"{pr.path_res}{test.Title}.txt"
				});
			}
			foreach (var test in pr.tests_ref)
			{
				results.Add(new ResultData()
				{
					Exe_id = pr.exe.Id,
					Test_id = test.Id,
					Path_res = $"{pr.path_res}{test.Title}.txt"
				});
			}

			dataContext.Results.AddRange(results);
			dataContext.SaveChanges();
			queue = queue.Append(pr).ToList();
			if (queue.Count() == 1)
				RunExecutables();
		}

		public async Task Delete(ExecutableData _executable)
		{
			if (queue.FirstOrDefault(i => i.exe.Id == _executable.Id) == null)
				return;
			executable = _executable;
			token.Cancel();
			await exec_ranner;
			token.Dispose();
			executable = null;
			if (queue.Count() > 0)
				RunExecutables();
		}

		public async Task Delete(IEnumerable<ExecutableData> executables)
		{
			del_queue = (from prog in queue
			join exe in executables on prog.exe.Id equals exe.Id
			select prog).ToList();
			if (del_queue.Count() == 0)
				return;
			token.Cancel();
			await exec_ranner;
			token.Dispose();
			del_queue = null;
			if (queue.Count() > 0)
				RunExecutables();
		}

		private class Program
		{
			public string path_res;
			public TestData[] tests_ref;
			public TestData[] tests;
			public ExecutableData exe;
			public string path_cmp;
		}


		static private Executor instance;
		private readonly string env;
		private Task exec_ranner;
		private CancellationToken c_token;
		private CancellationTokenSource token;
		private List<Program> queue;
		private List<Program> del_queue;
		private ExecutableData executable;

		private Executor()
		{
			env = MyEnvironment.GetInstance().Env;
			queue = new List<Program>();
		}

		private void RunExecutables()
		{
			token = new CancellationTokenSource();
			c_token = token.Token;
			exec_ranner = Task.Run(() =>
			{
				while (queue.Count() > 0)
				{
					StartTests(queue.First());
					if (c_token.IsCancellationRequested)
						break;
					queue.Remove(queue.First());
				}
			}, c_token);
		}

		private void StartTests(Program program)
		{
			var stat = new StreamWriter(env + program.exe.Path_stat);
			StartTests_NoRef(program, stat);
			if (c_token.IsCancellationRequested)
				return;
			StartTests_Ref(program, stat);
			stat.Close();
			bool is_deleted = MyEnvironment.GetInstance().DeleteFile(program.exe.Path_exe);
			while (!is_deleted)
				is_deleted = MyEnvironment.GetInstance().DeleteFile(program.exe.Path_exe);

		}

		private bool DefaultCMP(string path1, string path2)
		{
			var file1 = new StreamReader(path1);
			var file2 = new StreamReader(path2);
			var str1 = file1.ReadToEnd();
			var str2 = file2.ReadToEnd();
			file1.Close();
			file2.Close();
			return str1 == str2;
		}

		private bool CMP(Program pr, string path1, string path2)
		{
			if (!(new FileInfo(path1).Exists) || !(new FileInfo(path2).Exists))
				return false;
			if (pr.path_cmp == null)
				return DefaultCMP(path1, path2);
			Process process;
			try
			{
				process = Process.Start(env + pr.path_cmp, $"{path1} {path2}");
			}
			//catch (Win32Exception e)
			catch
			{
				return false;
			}
			
			process.WaitForExit();
			var y = process.ExitCode;
			return 1 == y;
		}

		private void StartTests_Ref(Program pr, StreamWriter stat)
		{
			if (pr.tests_ref.Count() == 0)
				return;

			var times = new int[pr.tests_ref.Count()];
			var is_completes = new bool[pr.tests_ref.Count()];
			stat.WriteLine("test_name; time; result");

			for (int i = 0; i < pr.tests_ref.Count(); ++i)
			{
				var test = env + pr.tests_ref[i].Path_test;
				var reference = env + pr.tests_ref[i].Path_reference;
				var file = new FileInfo(test);
				if (!file.Exists)
				{
					times[i] = 0;
					is_completes[i] = false;
					stat.WriteLine($"{ pr.tests_ref[i].Title}; { times[i]};{is_completes[i]};");
					continue;
				}
				//var file_catcher = file.OpenRead();
				var res = $"{pr.path_res}{pr.tests_ref[i].Title}.txt";

				Process process;
				try
				{
					process = Process.Start(env + pr.exe.Path_exe, $"4 {test} {env + res}");
				}
				catch (Win32Exception e)
				{
					stat.WriteLine("could not to execute this file");
					stat.WriteLine(e.Message);
					return;
				}
				while (!process.HasExited)
				{
					if (c_token.IsCancellationRequested)
					{
						process.Kill();
						process.Dispose();
						stat.Close();
						if (executable != null)
							queue.Remove(queue.FirstOrDefault(i => i.exe.Id == executable.Id));
						else
						{
							foreach (var prog in del_queue)
								queue.Remove(prog);
						}
						return;
					}
				}
				times[i] = process.UserProcessorTime.Milliseconds;
				process.Kill();
				process.Dispose();
				is_completes[i] = CMP(pr, env + res, reference);
				stat.WriteLine($"{ pr.tests_ref[i].Title}; { times[i]};{is_completes[i]};");
				//file_catcher.Close();
			}
		}

		private void StartTests_NoRef(Program pr, StreamWriter stat)
		{
			if (pr.tests.Count() == 0)
				return;

			var times = new int[pr.tests.Count()];
			stat.WriteLine("test_name; time");
			for (int i = 0; i < pr.tests.Count(); ++i)
			{
				var test = env + pr.tests[i].Path_test;
				var file = new FileInfo(test);
				if (!file.Exists)
				{
					times[i] = 0;
					stat.WriteLine($"{ pr.tests[i].Title}; { times[i]};");
					continue;
				}
				//var file_catcher = file.OpenRead();
				var res = $"{pr.path_res}{pr.tests[i].Title}.txt";
				Process process;
				try
				{
					process = Process.Start(env + pr.exe.Path_exe, $"4 {test} {env + res}");
				} catch (Win32Exception e)
				{
					stat.WriteLine("could not to execute this file");
					stat.WriteLine(e.Message);
					return;
				}
				//process.WaitForExit();
				while (!process.HasExited)
				{
					if (c_token.IsCancellationRequested)
					{
						process.Kill();
						process.Dispose();
						stat.Close();
						if (executable != null)
							queue.Remove(queue.FirstOrDefault(i => i.exe.Id == executable.Id));
						else
							foreach (var prog in del_queue)
								queue.Remove(prog);
						return;
					}
				}
				times[i] = process.UserProcessorTime.Milliseconds;
				process.Kill();
				process.Dispose();
				stat.WriteLine($"{ pr.tests[i].Title}; { times[i]};");
				//file_catcher.Close();
			}
		}
	}
}
