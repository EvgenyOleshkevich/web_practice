using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_practice.Models.DB;
using Web_practice.Models.Pages.Account;
using Web_practice.Models.Pages.Task;

namespace Web_practice.Controllers
{
    public class TaskController : Controller
    {
		#region Create New Task

		[HttpGet]
		public IActionResult AdditionTask()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new AdditionTaskModel());
		}

		[HttpPost]
		public async Task<IActionResult> AdditionTask(ProfileModel model)
		{
			return View(new AdditionTaskModel());
		}

		[HttpPost]
		public async Task<IActionResult> AdditionTask(TaskModel model)
		{
			return View(new AdditionTaskModel(model.Exe));
		}

		#endregion

		#region Task

		[HttpGet]
		public IActionResult Task()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new TaskModel());
		}


		[HttpPost]
		public async Task<IActionResult> Task(int id)
		{
			return View(new TaskModel(id));
		}
		#endregion

		#region Test

		[HttpGet]
		public IActionResult Test()
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new TestModel());
		}


		[HttpGet]
		public IActionResult TestRedaction(int id)
		{
			//logger.LogInformation("Visit /Account/Registration page");
			return View(new TestRedactionModel(new TestData() { Title = "tets " + id.ToString(), Id = id}));
		}

		[HttpPost]
		public async Task<IActionResult> Test(TestModel model)
		{
			//return View(new TestModel());
			return Redirect("Task");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteTest(int id)
		{
			//return View(new TestModel());
			return Redirect("Task");
		}
		#endregion
	}
}