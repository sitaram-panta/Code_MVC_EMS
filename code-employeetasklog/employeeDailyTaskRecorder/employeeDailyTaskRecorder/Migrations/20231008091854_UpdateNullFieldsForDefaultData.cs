using employeeDailyTaskRecorder.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employeeDailyTaskRecorder.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNullFieldsForDefaultData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(string.Format(@"
    Update [Employees]
    set 
        [CurrentStageCompletionDate] = getdate(),
        [EmpRole] = {0}, 
        [EmpStage] = {1}, 
        [Gender] = {2}, 
        [JoinDate] = getdate()
    where CurrentStageCompletionDate is null
", (int)EnumMajorRole.CustomerSupport_QA,
    (int)EnumEmployeeStage.Contractual,
    (int)EnumEmployeeGender.Male
    ));
            migrationBuilder.AlterColumn<string>(
    name: "ContactNumber",
    table: "Employees",
    type: "nvarchar(255)",
    nullable: false,
    defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CurrentStageCompletionDate",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "EmpRole",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "EmpStage",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinDate",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
