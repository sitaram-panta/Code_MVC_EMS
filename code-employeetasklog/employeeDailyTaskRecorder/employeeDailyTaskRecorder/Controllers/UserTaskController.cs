using Microsoft.AspNetCore.Mvc;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Models;
using Microsoft.EntityFrameworkCore;
using employeeDailyTaskRecorder.HelperService;
using employeeDailyTaskRecorder.CustomAttributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using employeeDailyTaskRecorder.StaticHelper;


namespace employeeDailyTaskRecorder.Controllers
{
    [GeneralAuthorization]
    public class UserTaskController : Controller
    {
        //private Employee empData;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UserTaskController> _logger;
        private Employee _ActiveUser => SessionService.GetSession(HttpContext);
        public UserTaskController(ApplicationDbContext db, ILogger<UserTaskController> logger) : base()
        {
            _db = db;
            _logger = logger;
        }
        [HttpGet]
        [HttpPost]
        //attributes so that only admin can access this page
        public async Task<IActionResult> Index(VMAdminIndex? SearchData)
        {
            try
            {
                VMAdminIndex Result = SearchData == null ? new VMAdminIndex() : SearchData;
                IQueryable<Record> recordList = _db.Records
                 .Include(x => x.Employee)
                 .Where(x =>
                    x.TaskPerformedDate.Date >= Result.FromDate.Date &&
                    x.TaskPerformedDate.Date <= Result.ToDate.Date
                    );
                if (SearchData.EmployeeID.HasValue)
                {
                    recordList = recordList.Where(x => x.EmployeeId == SearchData.EmployeeID.Value);
                }
                if (!string.IsNullOrEmpty(SearchData.SearchTerm))
                {
                    recordList = recordList.Where(x => x.Task.Contains(SearchData.SearchTerm));
                }
                bool HasOldRecord = _db.Records
                    .Any(x =>
                        x.EmployeeId == _ActiveUser.Id &&
                        x.TaskPerformedDate.Date == DateTime.Today.Date
                    );
                Result.CanAddTask = !HasOldRecord;
                Result.EmployeeList = _db.Employees.ToList();
                ViewBag.EmployeeList = new SelectList(Result.EmployeeList, "Id", "Name");
                TempData["activeUser"] = _ActiveUser.Id;
                TempData["activeUserName"] = SearchData.EmployeeName;
                Result.TaskList = await recordList.OrderByDescending(x => x.TaskPerformedDate).ToListAsync();
                return View(Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing user task");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        //return logged in user task
        [UserAuthorization]
        public IActionResult EmployeeTask()
        {
            try
            {
                VMAdminIndex Result = new VMAdminIndex();
                DateTime currentDateTime = DateTime.Now;
                string formattedDate = currentDateTime.ToString("MM/dd/yyyy");
                List<Record> records = _db.Records.Where(x => x.EmployeeId == _ActiveUser.Id).ToList();
                Result.CanAddTask = true;
                foreach (var data in records)
                {
                    if ((data.TaskPerformedDate.ToString("MM/dd/yyyy")).ToString() == formattedDate)
                    {
                        Result.CanAddTask = false;
                        break;
                    }
                }
                Result.EmployeeID = _ActiveUser.Id;
                Result.EmployeeName = _ActiveUser.Name;
                Result.EmployeeEmail = _ActiveUser.Email;
                Result.TaskList = _db.Records.Where(x => x.EmployeeId == _ActiveUser.Id).OrderByDescending(x => x.TaskPerformedDate).ToList();
                Result.EmployeeList = _db.Employees.Where(x => x.Id == _ActiveUser.Id).ToList();
                return View(Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing logged in user task");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult addTask(Record record)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                DateTime currentDateTime = DateTime.Now;
                Record value = new Record();
                value.Task = record.Task;
                value.TaskPerformedDate = currentDateTime;
                value.EmployeeId = _ActiveUser.Id;
                value.Ipaddress = ip;
                _db.Records.Add(value);
                _db.SaveChanges();
                if (_ActiveUser.IsAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("EmployeeTask");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding task");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult editTask(Record record, String Type)
        {
            try
            {
                Employee empData = SessionService.GetSession(HttpContext);
                Record value = _db.Records.Find(record.Id);
                value.Task = record.Task;
                _db.SaveChanges();
                if (empData.IsAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("EmployeeTask");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing task");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult deleteTask(int taskId)
        {
            try
            {
                Employee empData = SessionService.GetSession(HttpContext);
                Record data = _db.Records.Find(taskId);
                _db.Records.Remove(data);
                _db.SaveChanges();
                if (empData.IsAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("EmployeeTask");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting task");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }

        }
        public IActionResult TodaysTaskByEmp(DateTime? uDate)
        {
            try
            {
                uDate = uDate.HasValue ? uDate.Value : DateTime.Now;
                ViewBag.uDate = uDate.Value.ShowDate();

                var EmpList = _EmpList();
                List<Record> records = _db.Records
                    .Where(x => x.TaskPerformedDate.Date == uDate.Value.Date)
                    .ToList();
                var task = records;
                Parallel.ForEach(EmpList, item =>
                {
                    var rec = task.FirstOrDefault(x => x.EmployeeId == item.Id);

                    item.Records.Add(rec == null ? new Record() : rec);
                });
                return View(EmpList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing today's employee task");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        private IReadOnlyList<Employee> _EmpList()
        {
            return _db.Employees
                .AsNoTracking()
                .Where(x=>x.IsDeleted == false)
                .ToList();
        }
    }
}
