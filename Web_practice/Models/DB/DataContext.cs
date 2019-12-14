using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web_practice.Models.DB
{
	public class DataContext : DbContext
	{
		public DbSet<UserData> Users { get; set; }


		public DbSet<TaskData> Tasks { get; set; }

		public DbSet<ExecutableData> Exes { get; set; }

		public DbSet<TestData> Tests { get; set; }

		public DbSet<TaskAccessData> TaskAccesses { get; set; }

		public DbSet<StatisticData> Statistics { get; set; }

		public DbSet<ResltByParamData> ResltsByParam { get; set; }


		public DbSet<ResultData> Results { get; set; }

		public DataContext(DbContextOptions<DataContext> options)
			: base(options)
		{
			Database.EnsureCreated();
		}
	}
}
