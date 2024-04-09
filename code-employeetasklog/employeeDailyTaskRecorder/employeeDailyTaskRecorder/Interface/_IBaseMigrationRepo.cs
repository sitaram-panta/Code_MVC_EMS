using employeeDailyTaskRecorder.Data;



namespace employeeDailyTaskRecorder.Interface
{
    public interface _IBaseMigrationRepo<T> where T : class
    {
        void RawToDatabase(string ExcelFilePath, string RawTableName);
        IDictionary<T, string> RawDataProcess(string RawTableName);
        IDictionary<T, string> Migrate();
        ApplicationDbContext _db { get; }
        IQueryable<T> RawDataList();
    }
}
