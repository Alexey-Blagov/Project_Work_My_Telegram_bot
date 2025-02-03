using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Project_Work_My_Telegram_bot.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    IdTg = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TgUserName = table.Column<string>(type: "text", nullable: false),
                    UserRol = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    JobTitlel = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.IdTg);
                });

            migrationBuilder.CreateTable(
                name: "ObjectPaths",
                columns: table => new
                {
                    IdPath = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectName = table.Column<string>(type: "text", nullable: false),
                    PathLengh = table.Column<double>(type: "double precision", nullable: false),
                    DatePath = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CarId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectPaths", x => x.IdPath);
                    table.ForeignKey(
                        name: "FK_ObjectPaths_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "IdTg");
                });

            migrationBuilder.CreateTable(
                name: "OtherExpenses",
                columns: table => new
                {
                    ExpId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NameExpense = table.Column<string>(type: "text", nullable: false),
                    Coast = table.Column<decimal>(type: "numeric", nullable: false),
                    DateTimeExp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtherExpenses", x => x.ExpId);
                    table.ForeignKey(
                        name: "FK_OtherExpenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "IdTg");
                });

            migrationBuilder.CreateTable(
                name: "CarDrives",
                columns: table => new
                {
                    CarId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarName = table.Column<string>(type: "text", nullable: true),
                    isPersonalCar = table.Column<bool>(type: "boolean", nullable: false),
                    CarNumber = table.Column<string>(type: "text", nullable: false),
                    GasСonsum = table.Column<double>(type: "double precision", nullable: false),
                    TypeFuel = table.Column<int>(type: "integer", nullable: false),
                    IsPersonalKey = table.Column<long>(type: "bigint", nullable: true),
                    PathId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarDrives", x => x.CarId);
                    table.UniqueConstraint("AK_CarDrives_CarNumber", x => x.CarNumber);
                    table.ForeignKey(
                        name: "FK_CarDrives_ObjectPaths_PathId",
                        column: x => x.PathId,
                        principalTable: "ObjectPaths",
                        principalColumn: "IdPath");
                    table.ForeignKey(
                        name: "FK_CarDrives_Users_IsPersonalKey",
                        column: x => x.IsPersonalKey,
                        principalTable: "Users",
                        principalColumn: "IdTg");
                });

            migrationBuilder.CreateTable(
                name: "CarDriveUser",
                columns: table => new
                {
                    CarsCarId = table.Column<int>(type: "integer", nullable: false),
                    UserIdTg = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarDriveUser", x => new { x.CarsCarId, x.UserIdTg });
                    table.ForeignKey(
                        name: "FK_CarDriveUser_CarDrives_CarsCarId",
                        column: x => x.CarsCarId,
                        principalTable: "CarDrives",
                        principalColumn: "CarId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarDriveUser_Users_UserIdTg",
                        column: x => x.UserIdTg,
                        principalTable: "Users",
                        principalColumn: "IdTg",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarDrives_IsPersonalKey",
                table: "CarDrives",
                column: "IsPersonalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarDrives_PathId",
                table: "CarDrives",
                column: "PathId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarDriveUser_UserIdTg",
                table: "CarDriveUser",
                column: "UserIdTg");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectPaths_UserId",
                table: "ObjectPaths",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherExpenses_UserId",
                table: "OtherExpenses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarDriveUser");

            migrationBuilder.DropTable(
                name: "OtherExpenses");

            migrationBuilder.DropTable(
                name: "CarDrives");

            migrationBuilder.DropTable(
                name: "ObjectPaths");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
