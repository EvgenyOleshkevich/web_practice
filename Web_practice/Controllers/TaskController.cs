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
		[Obsolete]
		private readonly IHostingEnvironment appEnvironment;
		private readonly MyEnvironment environment;

		public TaskController(DataContext _dataContext,
			IDataProtectionProvider provider,
			IHostingEnvironment _appEnvironment)
		{
			ProtectData.GetInstance().Initialize(provider);
			dataContext = _dataContext;
			appEnvironment = _appEnvironment;
			environment = new MyEnvironment(dataContext, appEnvironment);
		}

		private async void CreateFileCode_onServer(IFormFile Code, string pathCode)
		{

			// сохраняем файл в папку data/ в каталоге wwwroot
			using (var fileStream = new FileStream(appEnvironment.WebRootPath + pathCode, FileMode.Create))
			{
				await Code.CopyToAsync(fileStream);
			}

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
		public async Task<IActionResult> AdditionTask(AdditionTaskModel model)
		{
			int user_id = GetUserId();

			if (dataContext.Tasks.FirstOrDefault(i => (i.User_id == user_id) && (i.Title == model.Title)) != null)
			{
				model.Errors.Add("задача с таким названием уже существует");
				ModelState.AddModelError("", "задача с таким названием уже существует");
				return View(model);
			}

			string nameDirectory = $"{user_id}/{model.Title}";

			Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}");
			Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}/tests");
			Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}/references");
			Directory.CreateDirectory($"./wwwroot/data/{nameDirectory}/executables");
			string pathCode;
			if (model.Comparator != null)
			{
				string nameFileCode = model.Comparator.FileName.Split("\\").Last(); //В IE имя это путь
																					// путь к папке /files/gameCode/{nameDirectory}/{nameFileCode}
				pathCode = $"/data/{nameDirectory}/{nameFileCode}"; //нужны 2 точки
				CreateFileCode_onServer(model.Comparator, pathCode);
			}
			else
				pathCode = "";

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
				task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);
				if (task.User_id != userId)
				{
					access = dataContext.TaskAccesses.FirstOrDefault(i => i.Task_id == taskId && i.User_id == userId).Level;
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
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);
			var model = new TaskModel()
			{
				Task = task,
				Tests = dataContext.Tests.Where(i => i.Task_id == task.Id).ToList(),
				AccessLevel = accessLevel
			};

			if (accessLevel % 10 == 1)
			{
				var user = dataContext.Users.FirstOrDefault(i => i.Id == userId).Login;
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
			environment.DeleteTask(task);
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
		public async Task<IActionResult> AdditionAccess(AdditionAccessModel model)
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

				if (dataContext.Users.FirstOrDefault(i => i.Id == ownUserId).Login == model.Login)
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
		public async Task<IActionResult> AccessChange(AccessChangeModel model, string accessIdEncode)
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
			var access = dataContext.TaskAccesses.FirstOrDefault(i => i.Id == accessId);
			var executables = dataContext.Exeсutables.Where(i => i.Task_id == access.Task_id
			&& i.User_id == access.User_id);

			foreach (var exe in executables)
				environment.DeleteExecutable(exe);
			dataContext.Exeсutables.RemoveRange(executables);
			dataContext.TaskAccesses.Remove(access);
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
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);

			if (dataContext.Tests.FirstOrDefault(i => i.Title == model.Title) != null)
			{
				model.Errors.Add("тест с таким названием уже существует");
				ModelState.AddModelError("", "тест с таким названием уже существует");
				return View(model);
			}

			string nameDirectoryTest = $"/data/{task.User_id}/{task.Title}/tests";
			string nameDirectoryRef = $"/data/{task.User_id}/{task.Title}/references";

			if (!Directory.Exists($"./wwwroot{nameDirectoryTest}"))
				Directory.CreateDirectory($"./wwwroot{nameDirectoryTest}");
			string nameFileTest = model.Test_path.FileName.Split("\\").Last();
			string pathTest = nameDirectoryTest + $"/{model.Title}_{nameFileTest}";
			CreateFileCode_onServer(model.Test_path, pathTest);

			string pathRef = null;
			if (model.Reference_path != null)
			{
				if (!Directory.Exists($"./wwwroot{nameDirectoryTest}"))
					Directory.CreateDirectory($"./wwwroot{nameDirectoryTest}");
				pathRef = nameDirectoryRef + $"/{model.Title}_{model.Reference_path.FileName.Split("\\").Last()}";
				CreateFileCode_onServer(model.Reference_path, pathRef);
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

		[HttpPost]
		public IActionResult TestDelete(string testIdEncode)
		{
			var testIdDecoded = ProtectData.GetInstance().DecodeToString(testIdEncode);
			var testId = Int32.Parse(testIdDecoded);
			var test = dataContext.Tests.FirstOrDefault(i => i.Id == testId);
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == test.Task_id);
			//string testFile = $"\\data\\{task.User_id}\\{task.Title}\\tests\\{test.Title}";


			environment.DeleteFile(test.Path_test);
			if (test.Path_reference != null)
			{
				string refFile = $"\\data\\{task.User_id}\\{task.Title}\\references\\{test.Title}";

				environment.DeleteFile(test.Path_reference);
			}
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
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);

			if (dataContext.Exeсutables.FirstOrDefault(i => (i.User_id == user_id)
			&& (i.Title == model.Title) && (i.Task_id == taskId)) != null)
			{
				model.Errors.Add("файл с таким названием уже существует");
				ModelState.AddModelError("", "файл с таким названием уже существует");
				return View(model);
			}

			string nameDirectory = $"/data/{task.User_id}/{task.Title}/executables/{user_id}/{model.Title}";

			Directory.CreateDirectory($"./wwwroot{nameDirectory}");
			string nameFileCode = model.Executable.FileName.Split("\\").Last(); //В IE имя это путь

			string pathCode = nameDirectory + $"/{nameFileCode}"; //нужны 2 точки
			CreateFileCode_onServer(model.Executable, pathCode);

			var exe = new ExecutableData
			{
				Title = model.Title,
				User_id = user_id,
				Path_exe = pathCode,
				Path_cmp = "",
				Task_id = taskId,
				InterpolationString = "",
				Iscalculating = false
			};
			dataContext.Exeсutables.Add(exe);
			dataContext.SaveChanges();
			return Redirect("Task");
		}

		[HttpPost]
		public IActionResult ExecutableDelete(string exeIdEncode)
		{
			var exeIdDecoded = ProtectData.GetInstance().DecodeToString(exeIdEncode);
			var exeId = Int32.Parse(exeIdDecoded);
			var exe = dataContext.Exeсutables.FirstOrDefault(i => i.Id == exeId);
			//string nameFile = $"\\data\\{task.User_id}\\{task.Title}\\executables\\{exe.User_id}\\{exe.Title}";
			environment.DeleteExecutable(exe);
			dataContext.Exeсutables.Remove(exe);
			dataContext.SaveChanges();

			return Redirect("Task");
		}
		#endregion
	}
}