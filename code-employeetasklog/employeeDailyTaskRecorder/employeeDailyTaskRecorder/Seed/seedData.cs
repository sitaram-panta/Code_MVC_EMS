using Microsoft.EntityFrameworkCore;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Models;
using employeeDailyTaskRecorder.Hash;

namespace employeeDailyTaskRecorder.Seed
{
    public class seedData
    {
        public static void Initialize(ApplicationDbContext _db)
        {
            try
            {

                _db.Database.Migrate();

                if (!_db.Employees.Any())
                {
                    _db.Employees.AddRange(
                        new Employee { Name = "Purna", Email = "admin@gmail.com", Address = "Kathmandu", Password = PwdEncryption.HashPassword("admin"), EmpType = EnumEmployeeType.Admin, ContactNumber = "1234" }
                    );

                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
