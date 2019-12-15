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

		public TaskController(DataContext _dataContext,
			IDataProtectionProvider provider,
			IHostingEnvironment _appEnvironment)
		{
			ProtectData.GetInstance().Initialize(provider);
			dataContext = _dataContext;
			appEnvironment = _appEnvironment;
		}

		private async void CreateFileCode_onServer(IFormFile Code, string pathCode)
		{

			// сохраняем файл в папку data/ в каталоге wwwroot
			using (var fileStream = new FileStream(appEnvironment.WebRootPath + pathCode, FileMode.Create))
			{
				await Code.CopyToAsync(fileStream);
			}

		}

		private void DeleteDirictory(string path)
		{
			//Path: /images/game/{nameDir}
			DirectoryInfo dirInfo = new DirectoryInfo(appEnvironment.WebRootPath + path);
			if (dirInfo.Exists)
			{
				DeleteDirictory(dirInfo);
				//Directory.Delete(appEnvironment.WebRootPath + path);
			}
		}

		private void DeleteDirictory(DirectoryInfo dirInfo)
		{
			foreach (var dir in dirInfo.GetDirectories())
				DeleteDirictory(dir);
			foreach (var file in dirInfo.GetFiles())
				file.Delete();
			dirInfo.Delete();
		}
		private void DeleteFile(string path)
		{
			//Path: /images/game/{nameDir}/{nameFile}
			var fileIcon = new FileInfo(appEnvironment.WebRootPath + path);
			if (fileIcon != null)
			{
				fileIcon.Delete();
			}
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
			int user_id = Int32.Parse(HttpContext.User.Identity.Name);

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
		public IActionResult Task(string taskIdEncode)
		{
			var taskIdDecoded = ProtectData.GetInstance().DecodeToString(taskIdEncode);
			var taskId = Int32.Parse(taskIdDecoded);
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);
			return View(new TaskModel
			{
				Task = task,
				Exes = dataContext.Exeсutables.Where(i => i.Task_id == task.Id).ToList(),
				Tests = dataContext.Tests.Where(i => i.Task_id == task.Id).ToList()
			});

		}


		[HttpPost]
		public async Task<IActionResult> TaskDelete(string taskIdEncode)
		{
			var taskIdDecoded = ProtectData.GetInstance().DecodeToString(taskIdEncode);
			var taskId = Int32.Parse(taskIdDecoded);
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);
			string nameDirectory = $"\\data\\{task.User_id}\\{task.Title}";
			DeleteDirictory(nameDirectory);
			dataContext.Tasks.Remove(task);
			dataContext.SaveChanges();

			return Redirect("~/Account/Profile");
		}

		#endregion

		#region Test

		[HttpGet]
		public IActionResult AdditionTest(string taskIdEncode)
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new AdditionTestModel(taskIdEncode));
		}

		[HttpPost]
		public IActionResult AdditionTest(AdditionTestModel model, string taskIdEncode)
		{
			int user_id = Int32.Parse(HttpContext.User.Identity.Name);
			var taskIdDecoded = ProtectData.GetInstance().DecodeToString(taskIdEncode);
			var taskId = Int32.Parse(taskIdDecoded);
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);

			if (dataContext.Tests.FirstOrDefault(i => i.Title == model.Title) != null)
			{
				model.Errors.Add("тест с таким названием уже существует");
				ModelState.AddModelError("", "тест с таким названием уже существует");
				model.TaskIdEncod = taskIdEncode;
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
			return Redirect("~/Account/Profile");
		}

		[HttpPost]
		public IActionResult TestDelete(string testIdEncode)
		{
			var testIdDecoded = ProtectData.GetInstance().DecodeToString(testIdEncode);
			var testId = Int32.Parse(testIdDecoded);
			var test = dataContext.Tests.FirstOrDefault(i => i.Id == testId);
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == test.Task_id);
			//string testFile = $"\\data\\{task.User_id}\\{task.Title}\\tests\\{test.Title}";


			DeleteFile(test.Path_test);
			if (test.Path_reference != null)
			{
				string refFile = $"\\data\\{task.User_id}\\{task.Title}\\references\\{test.Title}";

				DeleteFile(test.Path_reference);
			}
			dataContext.Tests.Remove(test);
			dataContext.SaveChanges();

			return Redirect("~/Account/Profile");
		}
		#endregion

		#region Executable

		[HttpGet]
		public IActionResult AdditionExecutable(string taskIdEncode)
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new AdditionExecutableModel(taskIdEncode));
		}

		[HttpPost]
		public IActionResult AdditionExecutable(AdditionExecutableModel model, string taskIdEncode)
		{
			int user_id = Int32.Parse(HttpContext.User.Identity.Name);
			var taskIdDecoded = ProtectData.GetInstance().DecodeToString(taskIdEncode);
			var taskId = Int32.Parse(taskIdDecoded);
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == taskId);

			if (dataContext.Exeсutables.FirstOrDefault(i => (i.User_id == user_id)
			&& (i.Title == model.Title) && (i.Task_id == taskId)) != null)
			{
				model.Errors.Add("файл с таким названием уже существует");
				ModelState.AddModelError("", "файл с таким названием уже существует");
				model.TaskIdEncod = taskIdEncode;
				return View(model);
			}

			string nameDirectory = $"/data/{task.User_id}/{task.Title}/executables/{user_id}";

			if (!Directory.Exists($"./wwwroot{nameDirectory}"))
				Directory.CreateDirectory($"./wwwroot{nameDirectory}");
			string nameFileCode = model.Executable.FileName.Split("\\").Last(); //В IE имя это путь

			string pathCode = nameDirectory + $"/{model.Title}_{nameFileCode}"; //нужны 2 точки
			CreateFileCode_onServer(model.Executable, pathCode);

			var exe = new ExecutableData
			{
				Title = model.Title,
				User_id = user_id,
				Path_exe = pathCode,
				Path_cmp = "",
				Task_id = taskId,
				InterpolationString = model.InterpolationString,
				Iscalculating = false
			};
			dataContext.Exeсutables.Add(exe);
			dataContext.SaveChanges();
			return Redirect("~/Account/Profile");
		}

		[HttpPost]
		public IActionResult ExecutableDelete(string exeIdEncode)
		{
			var exeIdDecoded = ProtectData.GetInstance().DecodeToString(exeIdEncode);
			var exeId = Int32.Parse(exeIdDecoded);
			var exe = dataContext.Exeсutables.FirstOrDefault(i => i.Id == exeId);
			var task = dataContext.Tasks.FirstOrDefault(i => i.Id == exe.Task_id);
			//string nameFile = $"\\data\\{task.User_id}\\{task.Title}\\executables\\{exe.User_id}\\{exe.Title}";
			DeleteFile(exe.Path_exe);
			dataContext.Exeсutables.Remove(exe);
			dataContext.SaveChanges();

			return Redirect("~/Account/Profile");
		}
		#endregion
	}
}