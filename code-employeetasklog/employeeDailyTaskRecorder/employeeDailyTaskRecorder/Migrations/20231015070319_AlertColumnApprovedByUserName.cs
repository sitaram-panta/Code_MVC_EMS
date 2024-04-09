using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employeeDailyTaskRecorder.Migrations
{
    /// <inheritdoc />
    public partial class AlertColumnApprovedByUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequest_Employees_EmployeeId",
                table: "LeaveRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaveRequest",
                table: "LeaveRequest");

            migrationBuilder.RenameTable(
                name: "LeaveRequest",
                newName: "leaveRequest");

            migrationBuilder.RenameIndex(
                name: "IX_LeaveRequest_EmployeeId",
                table: "leaveRequest",
                newName: "IX_leaveRequest_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_leaveRequest",
                table: "leaveRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_leaveRequest_Employees_EmployeeId",
                table: "leaveRequest",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<string>(
          name: "ApprovedByUserName",
          table: "LeaveRequest",
          type: "nvarchar(max)",
          nullable: true,
          oldClrType: typeof(string),
          oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_leaveRequest_Employees_EmployeeId",
                table: "leaveRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_leaveRequest",
                table: "leaveRequest");

            migrationBuilder.RenameTable(
                name: "leaveRequest",
                newName: "LeaveRequest");

            migrationBuilder.RenameIndex(
                name: "IX_leaveRequest_EmployeeId",
                table: "LeaveRequest",
                newName: "IX_LeaveRequest_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaveRequest",
                table: "LeaveRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequest_Employees_EmployeeId",
                table: "LeaveRequest",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<string>(
               name: "ApprovedByUserName",
               table: "LeaveRequest",
               type: "nvarchar(max)",
               nullable: false,
               defaultValue: "",
               oldClrType: typeof(string),
               oldType: "nvarchar(max)",
               oldNullable: true);
        }
    }
}
