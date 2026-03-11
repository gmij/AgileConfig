using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgileConfig.Server.Data.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agc_app",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Secret = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Group = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Creator = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_app", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_appInheritanced",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    appid = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    inheritanced_appid = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    Sort = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_appInheritanced", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_config",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    g = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    k = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    OnlineStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    EditStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Env = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_config", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_config_published",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    g = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    k = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PublishTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConfigId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    PublishTimelineId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Env = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_config_published", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_function",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SortIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_function", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_publish_detail",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    PublishTimelineId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    ConfigId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    Group = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EditStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Env = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_publish_detail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_publish_timeline",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    PublishTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PublishUserId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    PublishUserName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Log = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Env = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_publish_timeline", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_role",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_role_function",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    FunctionId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_role_function", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_server_node",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LastEchoTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_server_node", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_service_info",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    ServiceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ServiceName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Ip = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    MetaData = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RegisterTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastHeartBeat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HeartBeatMode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CheckUrl = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    AlarmUrl = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RegisterWay = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_service_info", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_setting",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_sys_log",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    LogType = table.Column<int>(type: "INTEGER", maxLength: 256, nullable: false),
                    LogTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LogText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_sys_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_user",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Password = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Salt = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    Team = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_user_app_auth",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Permission = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_user_app_auth", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agc_user_role",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RoleId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agc_user_role", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "agc_function",
                columns: new[] { "Id", "Category", "Code", "CreateTime", "Description", "Name", "SortIndex", "UpdateTime" },
                values: new object[,]
                {
                    { "001", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Add Application", "App.Add", 0, null },
                    { "002", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Edit Application", "App.Edit", 0, null },
                    { "003", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delete Application", "App.Delete", 0, null },
                    { "004", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Add Configuration", "Config.Add", 0, null },
                    { "005", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Edit Configuration", "Config.Edit", 0, null },
                    { "006", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delete Configuration", "Config.Delete", 0, null },
                    { "007", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Publish Configuration", "Config.Publish", 0, null },
                    { "008", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rollback Configuration", "Config.Rollback", 0, null },
                    { "009", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Add User", "User.Add", 0, null },
                    { "010", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Edit User", "User.Edit", 0, null },
                    { "011", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delete User", "User.Delete", 0, null },
                    { "012", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Add Node", "Node.Add", 0, null },
                    { "013", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Delete Node", "Node.Delete", 0, null },
                    { "014", null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "View System Log", "SysLog.View", 0, null }
                });

            migrationBuilder.InsertData(
                table: "agc_role",
                columns: new[] { "Id", "CreateTime", "Description", "IsSystem", "Name", "UpdateTime" },
                values: new object[,]
                {
                    { "001", new DateTime(2026, 3, 11, 9, 36, 20, 708, DateTimeKind.Local).AddTicks(5147), "System Administrator", true, "Administrator", null },
                    { "002", new DateTime(2026, 3, 11, 9, 36, 20, 708, DateTimeKind.Local).AddTicks(5436), "System Operator", true, "Operator", null }
                });

            migrationBuilder.InsertData(
                table: "agc_role_function",
                columns: new[] { "Id", "CreateTime", "FunctionId", "RoleId" },
                values: new object[,]
                {
                    { "08ed390e-ba8e-421b-a73d-35899646ba2b", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "010", "001" },
                    { "409e3398-def2-4132-92f3-700ff7448b04", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "006", "001" },
                    { "446cb7b4-99b5-4f0d-818b-a980629dfb3a", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "012", "001" },
                    { "4be2fe80-047b-4e27-b4c4-0351e483668a", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "011", "001" },
                    { "4cfc7e2e-9138-4ae9-a529-beaf0fcf96e1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "013", "001" },
                    { "595cd020-0c55-46e7-8a81-811a3451b32b", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "009", "001" },
                    { "5bb6742b-8952-488c-b5ae-e69aef018867", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "007", "001" },
                    { "6c7ed036-1eb7-407c-a897-2d1a3bc8c9d2", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "001", "001" },
                    { "6e2b69ca-3d9b-44db-b2df-6c818257a83f", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "005", "001" },
                    { "9fa97f7d-6806-4d58-9981-1d0801bbae4b", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "003", "001" },
                    { "b0c2f626-6d12-40fd-930d-d5fe0c3bbd56", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "004", "001" },
                    { "b913970b-81e1-4198-905b-d852de1a5057", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "014", "001" },
                    { "e56a2262-504d-42a0-beb0-76bff86dfe7e", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "008", "001" },
                    { "fdd8ff78-c875-45ee-b881-11da11b46f39", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "002", "001" }
                });

            migrationBuilder.InsertData(
                table: "agc_user",
                columns: new[] { "Id", "CreateTime", "Password", "Salt", "Source", "Status", "Team", "UpdateTime", "UserName" },
                values: new object[] { "admin", new DateTime(2026, 3, 11, 9, 36, 20, 706, DateTimeKind.Local).AddTicks(5170), "s7+8OVBvDjrupqv0jjamNQL9sszewpyqEv9cXnoJPfs=", "66941967e7ac4e74b0d87a8b4439fe73", 0, 0, "", null, "admin" });

            migrationBuilder.InsertData(
                table: "agc_user_role",
                columns: new[] { "Id", "CreateTime", "RoleId", "UserId" },
                values: new object[] { "3c13991d-c648-43e0-8323-e723242cc28e", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "001", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_agc_app_Name",
                table: "agc_app",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_agc_appInheritanced_appid",
                table: "agc_appInheritanced",
                column: "appid");

            migrationBuilder.CreateIndex(
                name: "IX_agc_appInheritanced_inheritanced_appid",
                table: "agc_appInheritanced",
                column: "inheritanced_appid");

            migrationBuilder.CreateIndex(
                name: "IX_agc_config_AppId_Env",
                table: "agc_config",
                columns: new[] { "AppId", "Env" });

            migrationBuilder.CreateIndex(
                name: "IX_agc_config_AppId_k_Env",
                table: "agc_config",
                columns: new[] { "AppId", "k", "Env" });

            migrationBuilder.CreateIndex(
                name: "IX_agc_config_published_AppId_Env",
                table: "agc_config_published",
                columns: new[] { "AppId", "Env" });

            migrationBuilder.CreateIndex(
                name: "IX_agc_config_published_PublishTimelineId",
                table: "agc_config_published",
                column: "PublishTimelineId");

            migrationBuilder.CreateIndex(
                name: "IX_agc_publish_detail_PublishTimelineId",
                table: "agc_publish_detail",
                column: "PublishTimelineId");

            migrationBuilder.CreateIndex(
                name: "IX_agc_publish_timeline_AppId_Env",
                table: "agc_publish_timeline",
                columns: new[] { "AppId", "Env" });

            migrationBuilder.CreateIndex(
                name: "IX_agc_publish_timeline_PublishTime",
                table: "agc_publish_timeline",
                column: "PublishTime");

            migrationBuilder.CreateIndex(
                name: "IX_agc_role_function_FunctionId",
                table: "agc_role_function",
                column: "FunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_agc_role_function_RoleId",
                table: "agc_role_function",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_agc_server_node_Status",
                table: "agc_server_node",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_agc_service_info_ServiceId_ServiceName",
                table: "agc_service_info",
                columns: new[] { "ServiceId", "ServiceName" });

            migrationBuilder.CreateIndex(
                name: "IX_agc_sys_log_LogTime",
                table: "agc_sys_log",
                column: "LogTime");

            migrationBuilder.CreateIndex(
                name: "IX_agc_user_UserName",
                table: "agc_user",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agc_user_app_auth_AppId_UserId",
                table: "agc_user_app_auth",
                columns: new[] { "AppId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_agc_user_role_RoleId",
                table: "agc_user_role",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_agc_user_role_UserId",
                table: "agc_user_role",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agc_app");

            migrationBuilder.DropTable(
                name: "agc_appInheritanced");

            migrationBuilder.DropTable(
                name: "agc_config");

            migrationBuilder.DropTable(
                name: "agc_config_published");

            migrationBuilder.DropTable(
                name: "agc_function");

            migrationBuilder.DropTable(
                name: "agc_publish_detail");

            migrationBuilder.DropTable(
                name: "agc_publish_timeline");

            migrationBuilder.DropTable(
                name: "agc_role");

            migrationBuilder.DropTable(
                name: "agc_role_function");

            migrationBuilder.DropTable(
                name: "agc_server_node");

            migrationBuilder.DropTable(
                name: "agc_service_info");

            migrationBuilder.DropTable(
                name: "agc_setting");

            migrationBuilder.DropTable(
                name: "agc_sys_log");

            migrationBuilder.DropTable(
                name: "agc_user");

            migrationBuilder.DropTable(
                name: "agc_user_app_auth");

            migrationBuilder.DropTable(
                name: "agc_user_role");
        }
    }
}
