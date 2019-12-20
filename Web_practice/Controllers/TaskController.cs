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
using Web_practice.Utilities;


namespace Web_practice.Controllers
{
	public class TaskController : Controller
	{
		private readonly DataContext dataContext;
		private readonly MyEnvironment environment;

		public TaskController(DataContext _dataContext,
			IDataProtectionProvider provider,
			IHostingEnvironment _appEnvironment)
		{
			ProtectData.GetInstance().Initialize(provider);
			dataContext = _dataContext;
			environment = MyEnvironment.GetInstance(dataContext);
		}
		private int GetUserId()
		{
			return Int32.Parse(HttpContext.User.Identity.Name);
		}

		private int GetTaskId()
		{
			return Int32.Parse(HttpContext.User.Claims.ToArray()[1].Value);
		}

		private int GetAccessLevel()
		{
			return Int32.Parse(HttpContext.User.Claims.Last().Value);
		}


		#region Create New Task
		[Authorize]
		[HttpGet]
		public IActionResult AdditionTask()
		{
			return View(new AdditionTaskModel());
		}

		[HttpPost]
		public IActionResult AdditionTask(AdditionTaskModel model)
		{
			int user_id = GetUserId();

			if (dataContext.Tasks.FirstOrDefault(i => (i.User_id == user_id) && (i.Title == model.Title)) != null)
			{
				model.Errors.Add("задача с таким названием уже существует");
				ModelState.AddModelError("", "задача с таким названием уже существует");
				return View(model);
			}

			string nameDirectory = $"{user_id}/{model.Title}";

			//Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}");
			//Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}/tests");
			//Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}/tests_ref");
			//Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}/references");
			//Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}/executables");

			environment.CreateDirectory(nameDirectory);
			environment.CreateDirectory(nameDirectory + "/tests");
			environment.CreateDirectory(nameDirectory + "/tests_ref");
			environment.CreateDirectory(nameDirectory + "/references");
			environment.CreateDirectory(nameDirectory + "/executables");
			string pathCode = null;
			if (model.Comparator != null)
			{
				string nameFileCode = model.Comparator.FileName.Split("\\").Last();
				pathCode = $"{nameDirectory}/{nameFileCode}";
				environment.CreateFileCode(model.Comparator, pathCode);
			}

			var task = new TaskData
			{
				Title = model.Title,
				User_id = user_id,
				Path_cmp = pathCode
			};
			dataContext.Tasks.Add(task);
			dataContext.SaveChanges();
			return Redirect("~/Account/Profile");
		}


		#endregion

		#region Task
		[HttpGet]
		public async Task<IActionResult> OpenTask(string taskIdEncode)
		{
			string taskId_str;
			int taskId;
			string userId_str;
			int userId;
			TaskData task;
			int access = 100;
			try
			{
				taskId_str = ProtectData.GetInstance().DecodeToString(taskIdEncode);
				taskId = Int32.Parse(taskId_str);
				userId_str = HttpContext.User.Identity.Name;
				userId = Int32.Parse(userId_str);
				task = dataContext.Tasks.Single(i => i.Id == taskId);
				if (task.User_id != userId)
				{
					access = dataContext.TaskAccesses.Single(i => i.Task_id == taskId && i.User_id == userId).Level;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				
					var claims = new List<Claim>
				{
					new Claim(ClaimsIdentity.DefaultNameClaimType, userId_str),
					new Claim(ClaimsIdentity.DefaultNameClaimType, taskId_str),
					new Claim(ClaimsIdentity.DefaultNameClaimType, access.ToString()),
				};

				ClaimsIdentity id = new ClaimsIdentity(
					claims,
					"ApplicationCoockie",
					ClaimsIdentity.DefaultNameClaimType,
					ClaimsIdentity.DefaultRoleClaimType
					);

				await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
				return Redirect("Task");
			}


		[HttpGet]
		public IActionResult Task()
		{
			//var taskIdDecoded = ProtectData.GetInstance().DecodeToString(taskIdEncode);
			var taskId = GetTaskId();
			var userId = GetUserId();
			var accessLevel = GetAccessLevel();
			var task = dataContext.Tasks.Single(i => i.Id == taskId);
			var model = new TaskModel()
			{
				Task = task,
				Tests = dataContext.Tests.Where(i => i.Task_id == task.Id).ToList(),
				AccessLevel = accessLevel
			};

			if (accessLevel % 10 == 1)
			{
				var user = dataContext.Users.Single(i => i.Id == userId).Login;
				//model.Exes = dataContext.Exeсutables.Where(i => i.Task_id == task.Id && i.User_id == userId).ToList();
				model.Exes = dataContext.Exeсutables.
							  Where(i => i.Task_id == taskId && i.User_id == userId).Select(
					i => new TaskModel.Execute
					{
						Exe = i,
						User = user

					}).ToList();
			}
			else
			{
				// model.Exes = dataContext.Exeсutables.Where(i => i.Task_id == task.Id).ToList();
				model.Exes = (from exe in dataContext.Exeсutables.Where(i => i.Task_id == taskId)
								join user in dataContext.Users on exe.User_id equals user.Id
								select new TaskModel.Execute
								{
									User = user.Login,
									Exe = exe
								}).ToList();
			}

			return View(model);
		}


		[HttpPost]
		public async Task<IActionResult> TaskDelete()
		{
			var taskId = GetTaskId();
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);
			if (task == null)
				return Redirect("~/Account/Profile");
			await environment.Delete(task);
			dataContext.SaveChanges();
			return Redirect("~/Account/Profile");
		}

		#endregion

		#region Access settings
		[HttpGet]
		public IActionResult TaskSettings()
		{
			var taskId = GetTaskId();
			var model = new TaskSettingsModel()
			{
				Followers = (from access in dataContext.TaskAccesses.Where(i => i.Task_id == taskId)
							 join user in dataContext.Users on access.User_id equals user.Id
							 select new TaskSettingsModel.Follower
							 {
								 User = user.Login,
								 Access = access
							 }).ToList()
			};

			return View(model);
		}


		[HttpGet]
		public IActionResult AdditionAccess()
		{
			return View(new AdditionAccessModel());
		}

		[HttpPost]
		public IActionResult AdditionAccess(AdditionAccessModel model)
		{

			if (ModelState.IsValid)
			{
				int ownUserId = GetUserId();
				var user = dataContext.Users.FirstOrDefault(i => i.Login == model.Login);
				if (user == null)
				{
					model.Errors.Add("Пользователь с таким логином не существует");
					ModelState.AddModelError("", "Пользователь с таким логином не существует");
				}

				if (dataContext.Users.Single(i => i.Id == ownUserId).Login == model.Login)
				{
					model.Errors.Add("Нельзя менять свой уровень доступа");
					ModelState.AddModelError("", "Нельзя менять свой уровень доступа");
				}

				if (!ModelState.IsValid)
				{
					return View(model);
				}
				else
				{
					var access = new TaskAccessData
					{
						User_id = user.Id,
						Task_id = GetTaskId(),
						Level = model.Level
					};

					try
					{
						dataContext.TaskAccesses.Add(access);
						dataContext.SaveChanges();
					}
					catch (Exception ex)
					{
						throw ex;
					}
					return RedirectToAction("Task");
				}
			}
			return View(model);
		}


		[HttpGet]
		public IActionResult AccessChange(string accessIdEncode)
		{
			return View(new AccessChangeModel(accessIdEncode));
		}

		[HttpPost]
		public IActionResult AccessChange(AccessChangeModel model, string accessIdEncode)
		{
			
			if (ModelState.IsValid)
			{
				var accessId = Int32.Parse(ProtectData.GetInstance().DecodeToString(accessIdEncode));
				var access = dataContext.TaskAccesses.FirstOrDefault(i => i.Id == accessId);

				if (access == null)
				{
					return View(model);
				}
				else
				{
					access.Level = model.Level;

					try
					{
						dataContext.Attach(access).State = EntityState.Modified;
						dataContext.SaveChanges();

					}
					catch (Exception ex)
					{
						throw ex;
					}
					return RedirectToAction("Task");
				}
			}
			return View(model);
		}


		[HttpPost]
		public async Task<IActionResult> AccessDelete(string accessIdEncode)
		{
			var accessIdDecoded = ProtectData.GetInstance().DecodeToString(accessIdEncode);
			var accessId = Int32.Parse(accessIdDecoded);
			var access = dataContext.TaskAccesses.Single(i => i.Id == accessId);

			if (access == null)
				return Redirect("~/Account/Profile");
			await environment.Delete(access);
			dataContext.SaveChanges();

			return Redirect("Task");
		}
		#endregion

		#region Test

		[HttpGet]
		public IActionResult AdditionTest()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new AdditionTestModel());
		}

		[HttpPost]
		public IActionResult AdditionTest(AdditionTestModel model)
		{
			int user_id = GetUserId();
			var taskId = GetTaskId();
			var task = dataContext.Tasks.Single(i => i.Id == taskId);

			if (dataContext.Tests.FirstOrDefault(i => i.Title == model.Title && i.Task_id == taskId) != null)
			{
				model.Errors.Add("тест с таким названием уже существует");
				ModelState.AddModelError("", "тест с таким названием уже существует");
				return View(model);
			}

			string pathTest;
			string pathRef = null;

			if (model.Reference_path != null)
			{
				string nameDirectoryTest = $"{task.User_id}/{task.Title}/tests_ref/";
				//string nameFileTest = model.Test_path.FileName.Split("\\").Last();
				string nameFile = model.Title + ".txt";
				//pathTest = nameDirectoryTest + $"{model.Title}_{nameFileTest}";
				pathTest = nameDirectoryTest + nameFile;

				string nameDirectoryRef = $"{task.User_id}/{task.Title}/references/";
				//string nameFileRef = model.Reference_path.FileName.Split("\\").Last();
				pathRef = nameDirectoryRef + nameFile;

				environment.CreateFileCode(model.Test_path, pathTest);
				environment.CreateFileCode(model.Reference_path, pathRef);
			}
			else
			{
				string nameDirectoryTest = $"{task.User_id}/{task.Title}/tests/";
				//string nameFileTest = model.Test_path.FileName.Split("\\").Last();
				string nameFile = model.Title + ".txt";
				pathTest = nameDirectoryTest + nameFile;

				environment.CreateFileCode(model.Test_path, pathTest);
			}

			var test = new TestData
			{
				Title = model.Title,
				Path_reference = pathRef,
				Path_test = pathTest,
				Task_id = taskId,
			};
			dataContext.Tests.Add(test);
			dataContext.SaveChanges();
			return Redirect("Task");
		}

		[HttpGet]
		public IActionResult Test(string testIdEncode)
		{
			var testIdDecoded = ProtectData.GetInstance().DecodeToString(testIdEncode);
			var testId = Int32.Parse(testIdDecoded);
			var test = dataContext.Tests.FirstOrDefault(i => i.Id == testId);
			if (test == null)
				return Redirect("Task");
			var reader = new StreamReader(environment.Env + test.Path_test);
			var model = new TestModel()
			{
				Test = test,
				TestContent = reader.ReadToEnd()
			};
			reader.Close();
			return View(model);
		}

		[HttpPost]
		public IActionResult TestDelete(string testIdEncode)
		{
			var testIdDecoded = ProtectData.GetInstance().DecodeToString(testIdEncode);
			var testId = Int32.Parse(testIdDecoded);
			var test = dataContext.Tests.FirstOrDefault(i => i.Id == testId);

			if (test == null || !environment.DeleteFile(test.Path_test))
			{
				return Redirect("Task");
			}
			if (test.Path_reference != null)
				while (!environment.DeleteFile(test.Path_reference))
				{ }

			dataContext.Tests.Remove(test);
			dataContext.SaveChanges();

			return Redirect("Task");
		}
		#endregion

		#region Executable

		[HttpGet]
		public IActionResult AdditionExecutable()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new AdditionExecutableModel());
		}

		[HttpPost]
		public IActionResult AdditionExecutable(AdditionExecutableModel model)
		{
			int user_id = GetUserId();
			//var taskIdDecoded = ProtectData.GetInstance().DecodeToString(taskIdEncode);
			var taskId = GetTaskId();
			var task = dataContext.Tasks.Single(i => i.Id == taskId);

			if (dataContext.Exeсutables.FirstOrDefault(i => (i.User_id == user_id)
			&& (i.Task_id == taskId) && (i.Title == model.Title)) != null)
			{
				model.Errors.Add("файл с таким названием уже существует");
				ModelState.AddModelError("", "файл с таким названием уже существует");
				return View(model);
			}

			string nameDirectory = $"{task.User_id}/{task.Title}/executables/{user_id}/{model.Title}";

			environment.CreateDirectory(nameDirectory);
			string nameFileCode = model.Executable.FileName.Split("\\").Last(); //В IE имя это путь

			string pathCode = nameDirectory + $"/{nameFileCode}"; //нужны 2 точки
			environment.CreateFileCode(model.Executable, pathCode);

			var exe = new ExecutableData
			{
				Title = model.Title,
				User_id = user_id,
				Path_exe = pathCode,
				Path_stat = nameDirectory + "/statistic.csv",
				Task_id = taskId
			};
			Executor.GetInstance().AddExecutable(task, exe, nameDirectory, dataContext);
			return Redirect("Task");
		}


		[HttpGet]
		public IActionResult Executable(string exeIdEncode)
		{
			var exeIdDecoded = ProtectData.GetInstance().DecodeToString(exeIdEncode);
			var exeId = Int32.Parse(exeIdDecoded);
			var exe = dataContext.Exeсutables.FirstOrDefault(i => i.Id == exeId);
			if (exe == null)
				return Redirect("Task");
			var results = dataContext.Results.Where(i => i.Exe_id == exeId);
			string statistic = "calculating process...";
			bool isCalculated = false;
			try
			{
				var reader = new StreamReader(environment.Env + exe.Path_stat);
				statistic = reader.ReadToEnd();
				reader.Close();
				isCalculated = !(new FileInfo(environment.Env + exe.Path_exe)).Exists;
			} catch (Exception e)
			{

			}
			var model = new ExecutableModel()
			{
				Executable = exe,
				Results = results,
				IsCalculated = isCalculated,
				Statistic = statistic
			};

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> ExecutableDelete(string exeIdEncode)
		{
			var exeIdDecoded = ProtectData.GetInstance().DecodeToString(exeIdEncode);
			var exeId = Int32.Parse(exeIdDecoded);
			var exe = dataContext.Exeсutables.FirstOrDefault(i => i.Id == exeId);
			if (exe == null)
				return Redirect("Task");
			await environment.Delete(exe);
			dataContext.SaveChanges();

			return Redirect("Task");
		}

		[HttpGet]
		public IActionResult Result(string resIdEncode)
		{
			var resIdDecoded = ProtectData.GetInstance().DecodeToString(resIdEncode);
			var resId = Int32.Parse(resIdDecoded);
			var res = dataContext.Results.FirstOrDefault(i => i.Id == resId);
			if (res == null)
				return Redirect("Task");

			string content = null;
			var file = new FileInfo(environment.Env + res.Path_res);
			if (file.Exists)
			{
				var reader = new StreamReader(environment.Env + res.Path_res);
				content = reader.ReadToEnd();
				reader.Close();
			}
			
			var model = new ResultModel()
			{
				Result = res,
				ResultContent = content
			};

			return View(model);
		}
		#endregion
	}
}