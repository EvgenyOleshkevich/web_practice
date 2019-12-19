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
			var builder = new ConfigurationBuilder();
			// ��������� ���� � �������� ��������
			builder.SetBasePath(Directory.GetCurrentDirectory());
			// �������� ������������ �� ����� appsettings.json
			builder.AddJsonFile("appsettings.json");
			// ������� ������������
			var config = builder.Build();
			// �������� ������ �����������
			string connectionString = config.GetConnectionString("DefaultConnection");

			var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
			var options = optionsBuilder
				.UseSqlServer(connectionString)
				.Options;
			var db = new DataContext(options);
			string env = "C:\\Users\\�������\\Desktop\\���������\\AM-MP.2semestr\\web_practice\\Web_practice\\wwwroot\\data\\";
			MyEnvironment.Init(db, env);
			Executor.Init(db); 
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
