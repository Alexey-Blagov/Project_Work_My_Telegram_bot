using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Project_Work_My_Telegram_bot.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarDrive",
                columns: table => new
                {
                    CarId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarName = table.Column<string>(type: "text", nullable: true),
                    isPersonalCar = table.Column<bool>(type: "boolean", nullable: false),
                    CarNumber = table.Column<string>(type: "text", nullable: false),
                    GasСonsum = table.Column<double>(type: "double precision", nullable: false),
                    TypeFuel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarDrive", x => x.CarId);
                    table.UniqueConstraint("AK_CarDrive_CarNumber", x => x.CarNumber);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    IdTg = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TgUserName = table.Column<string>(type: "text", nullable: false),
                    UserRol = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    JobTitlel = table.Column<string>(type: "text", nullable: true),
                    РersonalcarCarId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.IdTg);
                    table.ForeignKey(
                        name: "FK_User_CarDrive_РersonalcarCarId",
                        column: x => x.РersonalcarCarId,
                        principalTable: "CarDrive",
                        principalColumn: "CarId");
                });

            migrationBuilder.CreateTable(
                name: "ObjectPath",
                columns: table => new
                {
                    IdPath = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectName = table.Column<string>(type: "text", nullable: false),
                    PathLengh = table.Column<float>(type: "real", nullable: false),
                    DatePath = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CarId = table.Column<int>(type: "integer", nullable: true),
                    CarDriveCarId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectPath", x => x.IdPath);
                    table.ForeignKey(
                        name: "FK_ObjectPath_CarDrive_CarDriveCarId",
                        column: x => x.CarDriveCarId,
                        principalTable: "CarDrive",
                        principalColumn: "CarId");
                    table.ForeignKey(
                        name: "FK_ObjectPath_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "IdTg");
                });

            migrationBuilder.CreateTable(
                name: "OtherExpense",
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
                    table.PrimaryKey("PK_OtherExpense", x => x.ExpId);
                    table.ForeignKey(
                        name: "FK_OtherExpense_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "IdTg");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjectPath_CarDriveCarId",
                table: "ObjectPath",
                column: "CarDriveCarId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectPath_UserId",
                table: "ObjectPath",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OtherExpense_UserId",
                table: "OtherExpense",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_РersonalcarCarId",
                table: "User",
                column: "РersonalcarCarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectPath");

            migrationBuilder.DropTable(
                name: "OtherExpense");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "CarDrive");
        }
    }
}
