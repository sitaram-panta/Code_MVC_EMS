using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.HelperService;
using employeeDailyTaskRecorder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace employeeDailyTaskRecorder.Controllers
{
    public class LeaveRequestController : Controller
    {
        private readonly ApplicationDbContext _db;
        private Employee _ActiveUser => SessionService.GetSession(HttpContext);
        private readonly ILogger<LeaveRequestController> _logger;
        public LeaveRequestController(ApplicationDbContext db, ILogger<LeaveRequestController> logger)
        {
            _db = db;
            _logger = logger;
        }
        public IActionResult EmployeeLeaveRequest()
        {
            try
            {
                IList<LeaveRequest> result = _db.leaveRequest.Where(x => x.EmployeeId == _ActiveUser.Id).Include(x => x.Employee).ToList();
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing logged in user leave requests");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult NewLeaveRequest()
        {
            try
            {
                var newLeaveRequestList = _db.leaveRequest.Where(x => x.ApprovedByUserName == null).Include(x => x.Employee).ToList();
                return View(newLeaveRequestList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing new leave request");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult RequestHistory()
        {
            try
            {
                var requestHistoryList = _db.leaveRequest.Where(x => x.ApprovedByUserName != null).Include(x => x.Employee).ToList();
                return View(requestHistoryList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing leave request history ");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult LeaveRequestReport(int? filterYearName,int? userId)
        {
            try
            {
                ViewBag.EmployeeList = new SelectList(_db.Employees.Where(x=>x.EmpType == EnumEmployeeType.Employee), "Id", "Name");
                ViewBag.FilterYearName = filterYearName ?? DateTime.Now.Year;
                var EmployeeList = _db.Employees.Where(x => x.EmpType == EnumEmployeeType.Employee).Include(x => x.LeaveRequests).ToList();
                if(userId != null)
                {
                    EmployeeList = _db.Employees.Where(x => x.Id == userId).ToList();
                }
                return View(EmployeeList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retuirng leave request report");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult CreateLeaveRequest(LeaveRequest leaveRequest)
        {
            try
            {
                LeaveRequest value = new LeaveRequest();
                value.EmployeeId = _ActiveUser.Id;
                value.FromDate = leaveRequest.FromDate;
                value.ToDate = leaveRequest.ToDate;
                value.LeaveRemarks = leaveRequest.LeaveRemarks;
                _db.leaveRequest.Add(value);
                _db.SaveChanges();
                return RedirectToAction("EmployeeLeaveRequest");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating leave request");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult DeleteLeaveRequest(int Id)
        {
            try
            {
                LeaveRequest value = _db.leaveRequest.Find(Id);
                _db.leaveRequest.Remove(value);
                _db.SaveChanges();
                return RedirectToAction("EmployeeLeaveRequest");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting leave request");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult EditLeaveRequest(int Id, LeaveRequest leaveRequest)
        {
            try
            {
                LeaveRequest value = _db.leaveRequest.Find(Id);
                value.EmployeeId = _ActiveUser.Id;
                value.FromDate = leaveRequest.FromDate;
                value.ToDate = leaveRequest.ToDate;
                value.LeaveRemarks = leaveRequest.LeaveRemarks;
                _db.SaveChanges();
                return RedirectToAction("EmployeeLeaveRequest");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing leave request");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
     
        public IActionResult AcceptLeaveRequest(int Id)
        {
            try
            {
                var leaveRequestValue = _db.leaveRequest.Find(Id);
                leaveRequestValue.ApprovedByUserName = _ActiveUser.Name;
                leaveRequestValue.ApprovedDate = DateTime.Now;
                leaveRequestValue.IsApproved = true;
                _db.SaveChanges();
                return RedirectToAction("NewLeaveRequest");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while accepting leave request");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult RejectLeaveRequest(int Id) 
        {
            try
            {
                var leaveRequestValue = _db.leaveRequest.Find(Id);
                leaveRequestValue.ApprovedByUserName = _ActiveUser.Name;
                leaveRequestValue.ApprovedDate = DateTime.Now;
                _db.SaveChanges();
                return RedirectToAction("NewLeaveRequest");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rejecting leave request");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
    }
}
