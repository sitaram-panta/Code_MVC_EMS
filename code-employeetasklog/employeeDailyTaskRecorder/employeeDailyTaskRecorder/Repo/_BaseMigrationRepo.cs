    using employeeDailyTaskRecorder.Data;
    using employeeDailyTaskRecorder.Interface;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Data.OleDb;
    using System.Linq;

    namespace employeeDailyTaskRecorder.Repository
    {
        public abstract class _BaseMigrationRepo<T> : _IBaseMigrationRepo<T>
            where T : class
        {
            public ApplicationDbContext _db { get; protected set; }

            public _BaseMigrationRepo(ApplicationDbContext dbContext)
            {
                _db = dbContext;
            }

            public abstract IDictionary<T, string> Migrate();
            public abstract IQueryable<T> RawDataList();
            public abstract IDictionary<T, string> RawDataProcess(string RawTableName);
            public abstract void RawToDatabase(string ExcelFilePath, string RawTableName);

            public virtual void SaveFile2DB(string FilePath, string SQL, string RawTableName)
            {
            String excelConnString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=D:\\Excel sheet\\Excel.xls;Extended Properties=\"Excel 12.0 Xml;HDR=Yes;IMEX=1;\"";


                using (OleDbConnection excelConnection = new OleDbConnection(excelConnString))
                {
                    excelConnection.Open();
                    string myTableName = excelConnection.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();

                    using (OleDbCommand cmd = new OleDbCommand(string.Format(SQL, myTableName), excelConnection))
                    {
                        using (OleDbDataReader dReader = cmd.ExecuteReader())
                        {
                            if (_db.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                            {
                                _db.Database.GetDbConnection().Open();
                            }
                            using (SqlBulkCopy sqlBulk = new SqlBulkCopy((SqlConnection)_db.Database.GetDbConnection()))
                            {
                                sqlBulk.DestinationTableName = RawTableName;
                                MapMigrationColumns(sqlBulk);
                                sqlBulk.WriteToServer(dReader);
                            }
                        }
                    }
                }
            }

            public abstract void MapMigrationColumns(SqlBulkCopy sqlBulk);
        }
    }
