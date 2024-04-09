using Microsoft.AspNetCore.Mvc;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Models;
using Microsoft.EntityFrameworkCore;
using employeeDailyTaskRecorder.HelperService;
using employeeDailyTaskRecorder.Hash;
namespace employeeDailyTaskRecorder.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor contxt;
        public AuthController(ILogger<AuthController> logger, ApplicationDbContext db)
        {

            _db = db;
            _logger = logger;
        }
        public IActionResult Index()
        {
            try { 
            return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returning login page");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult authenticateUser(String email, String password)
        {
            try
            {
                if (password != null && email != null)
                {
                    Employee EmpData = _db.Employees.FirstOrDefault(x => x.Email == email && x.IsDeleted == false);
                    if (EmpData == null)
                    {

                        TempData["ErrorMessage"] = "Incorrect Email";
                        return RedirectToAction("Index", "Auth");
                    }
                    var passwordValue = EmpData.Password;
                    if (passwordValue != PwdEncryption.HashPassword(password))
                    {
                        TempData["ErrorMessage"] = "Incorrect password.";
                        return RedirectToAction("Index", "Auth");
                    }
                    SessionService.SetSession(EmpData, HttpContext);

                    if (EmpData.IsAdmin)
                    {
                        return RedirectToAction("Dashboard", "Admin");

                    }
                    return RedirectToAction("EmployeeTask", "UserTask");
                }
                TempData["ErrorMessage"] = "Empty Field";
                return RedirectToAction("Index", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking login user email and password");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }

        }
        public IActionResult Logout()
        {
            try
            {
                SessionService.ClearSession(HttpContext);
                return RedirectToAction("Index", "Auth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging out");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
    }
}
