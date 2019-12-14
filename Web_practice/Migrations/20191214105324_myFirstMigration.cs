using Microsoft.EntityFrameworkCore.Migrations;

namespace Web_practice.Migrations
{
	public partial class myFirstMigration : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Exes",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					User_id = table.Column<int>(nullable: false),
					Task_id = table.Column<int>(nullable: false),
					Title = table.Column<string>(maxLength: 100, nullable: false),
					Path_exe = table.Column<string>(maxLength: 255, nullable: false),
					Path_cmp = table.Column<string>(maxLength: 255, nullable: false),
					Iscalculating = table.Column<bool>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Exes", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ResltsByParam",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Stat_id = table.Column<int>(nullable: false),
					Test_id = table.Column<int>(nullable: false),
					Path_res = table.Column<string>(maxLength: 255, nullable: false),
					Param = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ResltsByParam", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Results",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Stat_id = table.Column<int>(nullable: false),
					Test_id = table.Column<int>(nullable: false),
					Path_res = table.Column<string>(maxLength: 255, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Results", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Statistics",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Exe_id = table.Column<int>(nullable: false),
					Path_stat = table.Column<string>(maxLength: 255, nullable: false),
					Param_from = table.Column<int>(nullable: false),
					Param_to = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Statistics", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "TaskAccesses",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					User_id = table.Column<int>(nullable: false),
					Task_id = table.Column<int>(nullable: false),
					Level = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TaskAccesses", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Tests",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Task_id = table.Column<int>(nullable: false),
					Title = table.Column<string>(maxLength: 100, nullable: false),
					Path_test = table.Column<string>(maxLength: 255, nullable: false),
					Path_reference = table.Column<string>(maxLength: 255, nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tests", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Login = table.Column<string>(maxLength: 100, nullable: false),
					Password = table.Column<string>(maxLength: 100, nullable: false),
					Email = table.Column<string>(maxLength: 100, nullable: false),
					PathAvatar = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Tasks",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					User_id = table.Column<int>(nullable: false),
					Title = table.Column<string>(maxLength: 100, nullable: false),
					Path_cmp = table.Column<string>(maxLength: 255, nullable: false),
					UserDataId = table.Column<int>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tasks", x => x.Id);
					table.ForeignKey(
						name: "FK_Tasks_Users_UserDataId",
						column: x => x.UserDataId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_UserDataId",
				table: "Tasks",
				column: "UserDataId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Exes");

			migrationBuilder.DropTable(
				name: "ResltsByParam");

			migrationBuilder.DropTable(
				name: "Results");

			migrationBuilder.DropTable(
				name: "Statistics");

			migrationBuilder.DropTable(
				name: "TaskAccesses");

			migrationBuilder.DropTable(
				name: "Tasks");

			migrationBuilder.DropTable(
				name: "Tests");

			migrationBuilder.DropTable(
				name: "Users");
		}
	}
}
// add-migrate ..
// update database