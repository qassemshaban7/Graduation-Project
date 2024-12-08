using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GP_MVC.Migrations
{
    /// <inheritdoc />
    public partial class SurveysManag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThirdAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FourthAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SurveyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SurveyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Results_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "jg57hyr5-0765-464d-a42b-185922feb101",
                column: "ConcurrencyStamp",
                value: "0b282e56-f2f5-4919-b307-e3023656c1a2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "kh51dg85-0765-464d-a42b-185922f14522",
                column: "ConcurrencyStamp",
                value: "d3037cad-7409-45f6-b8b8-17a9295e4cd3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "lsg55434-0765-464d-a42b-18524ffeb108",
                column: "ConcurrencyStamp",
                value: "f35e3134-81e2-4140-9659-74bcaa75d369");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "zs08h2c5-0765-464d-a42b-185922jfdjk5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b2399c23-f630-4670-be61-b507d2bc5c99", "AQAAAAIAAYagAAAAEHkCDJ1gFaeP/IxioElsb6S9Skvedl//IR2zU3N3jPEdOJbnMoJzaWaZt7RSar8L4Q==", "c0383de8-0caf-4b94-984d-a0d85db3270d" });

            migrationBuilder.UpdateData(
                table: "Terms",
                keyColumn: "Id",
                keyValue: 1,
                column: "EndDate",
                value: new DateTime(2024, 6, 19, 23, 17, 47, 589, DateTimeKind.Local).AddTicks(3984));

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SurveyId",
                table: "Questions",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ApplicationUserId",
                table: "Results",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_QuestionId",
                table: "Results",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_SurveyId",
                table: "Results",
                column: "SurveyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Surveys");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "jg57hyr5-0765-464d-a42b-185922feb101",
                column: "ConcurrencyStamp",
                value: "17f57108-9777-4520-988e-dd7151fde381");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "kh51dg85-0765-464d-a42b-185922f14522",
                column: "ConcurrencyStamp",
                value: "1a2c42aa-e26e-4b2c-946c-facf28a0fe43");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "lsg55434-0765-464d-a42b-18524ffeb108",
                column: "ConcurrencyStamp",
                value: "c33dfb69-8ad9-40e0-9341-1f7db9b7c909");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "zs08h2c5-0765-464d-a42b-185922jfdjk5",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0d346f91-9102-4070-83b3-12b76918eb71", "AQAAAAIAAYagAAAAEO4gdjuImomA+1ltR2pTW5kpIGQ083kqjA2GFuS+F7rQbKeO8ldKsHAEpGKnKVP3mg==", "27c929ba-7912-486e-8045-680be350b052" });

            migrationBuilder.UpdateData(
                table: "Terms",
                keyColumn: "Id",
                keyValue: 1,
                column: "EndDate",
                value: new DateTime(2024, 6, 5, 19, 10, 54, 832, DateTimeKind.Local).AddTicks(6655));
        }
    }
}
