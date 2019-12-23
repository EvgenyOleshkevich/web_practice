using Microsoft.EntityFrameworkCore;

namespace Web_practice.Models.DB
{
	public class DataContext : DbContext
	{
		public DbSet<UserData> Users { get; set; }

		public DbSet<TaskData> Tasks { get; set; }
		public DbSet<TestData> Tests { get; set; }

		public DbSet<TaskAccessData> TaskAccesses { get; set; }

		public DbSet<ExecutableData> Exeсutables { get; set; }

		public DbSet<QueueData> Queue { get; set; }

		public DbSet<ResultData> Results { get; set; }

		public DataContext(DbContextOptions<DataContext> options)
			: base(options)
		{
			Database.EnsureCreated();
		}
	}
}
