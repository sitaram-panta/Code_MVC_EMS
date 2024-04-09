using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Interface;
using employeeDailyTaskRecorder.Models;
using System.Data.OleDb;

namespace employeeDailyTaskRecorder.Repository
{
    public class EmployeeMigrationRepo : _BaseMigrationRepo<EmployeeMigration>, IEmployeeMigrationRepo
    {
        private readonly IConfiguration _configuration;

        public EmployeeMigrationRepo(ApplicationDbContext db, IConfiguration configuration) : base(db)
        {
            _configuration = configuration;
        }

        public override IDictionary<EmployeeMigration, string> Migrate()
        {
            IDictionary<EmployeeMigration, string> FailList = new Dictionary<EmployeeMigration, string>();
            IList<EmployeeMigration> DataList = RawDataList().ToList();

            foreach (var item in DataList)
            {
                try
                {
                    var oldRec = _db.mig_Employees.FirstOrDefault(e => e.Id == item.Id);
                    if (oldRec != null)
                    {
                        // Update the existing record with data from the Excel file.
                        oldRec.Name = item.Name.Trim();
                        oldRec.Email = item.Email.Trim();
                        oldRec.Address = item.Address.Trim();
                        oldRec.ContactNumber = item.ContactNumber;
                        oldRec.Gender = item.Gender;
                        oldRec.EmpStage = item.EmpStage;
                        oldRec.EmpRole = item.EmpRole;
                        oldRec.JoinDate = item.JoinDate;
                        oldRec.EmpType = item.EmpType;
                    }
                    else
                    {
                        // If the item does not exist in the database, create a new one.
                        _db.mig_Employees.Add(item);
                    }

                    _db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    FailList.Add(item, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                }
                catch (Exception ex)
                {
                    FailList.Add(item, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                }
            }

            return FailList;
        }

        public override IDictionary<EmployeeMigration, string> RawDataProcess(string RawTableName)
        {
            IDictionary<EmployeeMigration, string> FailList = new Dictionary<EmployeeMigration, string>();
            // Implement any preprocessing logic here if needed.

            return FailList;
        }

        public override void RawToDatabase(string ExcelFilePath, string RawTableName)
        {
            _db.Database.ExecuteSqlRaw($"Truncate Table [{RawTableName}]");
            SaveFile2DB(ExcelFilePath, @"
                Select 
                [Id] as Id
                , [name] as name
                , [Email] as Email
                , [Address] as Address
                , [ContactNumber] as ContactNumber
                , [Gender] as Gender
                , [EmpStage] as EmpStage
                , [EmpRole] as EmpRole
                , [JoinDate] as JoinDate
                , [EmpType] as EmpType
                from [{0}]", RawTableName);
        }

        public override IQueryable<EmployeeMigration> RawDataList()
        {
            return _db.mig_Employees.AsQueryable();
        }

        public override void MapMigrationColumns(SqlBulkCopy sqlBulk)
        {
            // Define column mappings one-to-one.
            sqlBulk.ColumnMappings.Add("Id", "Id");
            sqlBulk.ColumnMappings.Add("Name", "Name");
            sqlBulk.ColumnMappings.Add("Email", "Email");
            sqlBulk.ColumnMappings.Add("Address", "Address");
            sqlBulk.ColumnMappings.Add("ContactNumber", "ContactNumber");
            sqlBulk.ColumnMappings.Add("Gender", "Gender");
            sqlBulk.ColumnMappings.Add("EmpStage", "EmpStage");
            sqlBulk.ColumnMappings.Add("EmpRole", "EmpRole");
            sqlBulk.ColumnMappings.Add("JoinDate", "JoinDate");
            sqlBulk.ColumnMappings.Add("EmpType", "EmpType");
        }

        public IDictionary<EmployeeMigration, string> MigrateDataFromExcel(string MainFilePath)
        {
            IDictionary<EmployeeMigration, string> FailList = new Dictionary<EmployeeMigration, string>();

            // Configure the Excel connection string from appsettings.json
            string excelConnectionString = _configuration.GetConnectionString("ExcelConnection");

            using (OleDbConnection excelConnection = new OleDbConnection(excelConnectionString))
            {
                try
                {
                    excelConnection.Open();
                    string sheetName = excelConnection.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();

                    using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM [{sheetName}]", excelConnection))
                    {
                        using (OleDbDataReader dReader = cmd.ExecuteReader())
                        {
                            if (_db.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                            {
                                _db.Database.GetDbConnection().Open();
                            }

                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy((SqlConnection)_db.Database.GetDbConnection()))
                            {
                                sqlBulk.DestinationTableName = sheetName;
                                MapMigrationColumns(sqlBulk);
                                sqlBulk.WriteToServer(dReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    FailList.Add(null, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                }
                finally
                {
                    excelConnection.Close();
                }
            }

            return FailList;
        }
    }
}
