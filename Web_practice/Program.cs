using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Web_practice.Models.DB;
using Web_practice.Utilities;

namespace Web_practice
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//string env = "C:\\Users\\Евгений\\Desktop\\Программы\\AM-MP.2semestr\\web_practice\\Web_practice\\wwwroot\\data\\";
			//MyEnvironment.Init(env);
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
