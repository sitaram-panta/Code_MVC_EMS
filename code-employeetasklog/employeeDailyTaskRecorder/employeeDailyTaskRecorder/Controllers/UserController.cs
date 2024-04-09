using Microsoft.AspNetCore.Mvc;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Models;
using employeeDailyTaskRecorder.HelperService;
using employeeDailyTaskRecorder.CustomAttributes;
using employeeDailyTaskRecorder.Hash;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using NuGet.Protocol;

namespace employeeDailyTaskRecorder.Controllers
{
    [GeneralAuthorization]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UserController> _logger;
        public UserController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, ILogger<UserController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _db = db;
            _logger = logger;
        }
        public IActionResult Index()
        {
            try
            {
                VMAdminIndex Result = new VMAdminIndex();
                Result.EmployeeList = _db.Employees.Where(x => x.IsDeleted == false).ToList();
                return View(Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returning user page");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult FormerEmployee()
        {
            try
            {
                VMAdminIndex Result = new VMAdminIndex();
                Result.EmployeeList = _db.Employees.Where(x => x.IsDeleted == true).ToList();
                return View(Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returning formal employee");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        [AdminAuthorization]
        public IActionResult UserDetails(int id)
        {
            try
            {
                VMAdminIndex Result = new VMAdminIndex();
                Result.TaskList = _db.Records.Where(x => x.EmployeeId == id).ToList();
                Result.EmployeeList = _db.Employees.Where(x => x.Id == id).ToList();
                return View(Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing user details");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult addUser(Employee employee)
        {
            try
            {
                var emailList = _db.Employees.Where(x => x.Email == employee.Email).FirstOrDefault();
                if (emailList != null)
                {
                    TempData["emailPresence"] = true;
                }
                else
                {
                    Employee value = new Employee();
                    value.Name = employee.Name;
                    value.Address = employee.Address;
                    value.ContactNumber = employee.ContactNumber;
                    value.Gender = employee.Gender;
                    value.EmpRole = employee.EmpRole;
                    value.EmpStage = employee.EmpStage;
                    value.JoinDate = employee.JoinDate;
                    value.CurrentStageCompletionDate = employee.CurrentStageCompletionDate;
                    value.Email = employee.Email;
                    value.Password = PwdEncryption.HashPassword(employee.Password);
                    value.EmpType = employee.EmpType;
                    value.ProfileImg = "";
                    _db.Employees.Add(value);
                    _db.SaveChanges();
                    TempData["emailPresence"] = false;

                }
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding user");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult editUser(Employee employee)
        {
            try
            {
                Employee value = _db.Employees.Find(employee.Id);
                if (value == null) { throw new Exception("Cannot Find Related data in db"); }
                value.Name = employee.Name;
                value.Address = employee.Address;
                value.Email = employee.Email;
                value.Password = employee.Password;
                value.EmpType = employee.EmpType;
                _db.SaveChanges();
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing user");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }

        public IActionResult deleteUser(int employeeId)
        {
            try
            {
                Employee data = _db.Employees.Find(employeeId);
                data.IsDeleted = true;
                _db.SaveChanges();
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult deleteUserPermanently(int employeeId)
        {
            try
            {
                Employee data = _db.Employees.Find(employeeId);
                _db.Employees.Remove(data);
                _db.SaveChanges();
                return RedirectToAction("FormerEmployee");

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user permanently");
                return RedirectToAction("TryCatchError", "ErrorHandling");
            }
        }
        public IActionResult EmployeeEditProfile(int id)
        {
            try
            {
                Employee empData = SessionService.GetSession(HttpContext);
                VMValidatePassword UserData = new VMValidatePassword();
                UserData.EmployeeID = empData.Id;
                UserData.EmployeeList = _db.Employees.Where(x => x.Id == id).ToList();
                return View(UserData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing employee profile");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult editProfileImg(int Id, IFormFile? profileImg)
        {
            try
            {
                Employee data = _db.Employees.Find(Id);
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (profileImg != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImg.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"Img\Upload");
                    string tempFilePath = Path.Combine(productPath, "temp_" + fileName);
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        profileImg.CopyTo(fileStream);
                    }
                    using (var imageStream = new FileStream(tempFilePath, FileMode.Open))
                    {
                        using (var image = Image.Load(imageStream))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Size = new Size(1024, 600),
                                Mode = ResizeMode.Max
                            }));
                            image.Save(Path.Combine(productPath, fileName), new JpegEncoder
                            {
                                Quality = 70
                            });
                        }
                    }

                    System.IO.File.Delete(tempFilePath);
                    data.ProfileImg = @"/Img/Upload/" + fileName;
                }
                _db.SaveChanges();
                return RedirectToAction("EmployeeEditProfile", "User", new { id = Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing profile image");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }

        public IActionResult deleteProfileImg(int Id)
        {
            try
            {
                Employee data = _db.Employees.Find(Id);
                data.ProfileImg = "";
                _db.SaveChanges();
                return RedirectToAction("EmployeeEditProfile", "User", new { id = Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting profile image");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult EditEmployeePassword(VMValidatePassword UserData)
        {
            try
            {
                bool flag = true;
                if (ModelState.IsValid)
                {
                    Employee value = _db.Employees.Find(UserData.EmployeeID);
                    if (value.Password != PwdEncryption.HashPassword(UserData.OldPassword))
                    {
                        TempData["EditPasswordMessage"] = "Old password doesn't match";
                        flag = false;
                    }
                    else if (UserData.ConfirmPassword != UserData.NewPassword)
                    {

                        TempData["EditPasswordMessage"] = "New password and confirm password doesn't match";
                        flag = false;
                    }
                    else
                    {
                        value.Password = PwdEncryption.HashPassword(UserData.ConfirmPassword);
                        _db.SaveChanges();
                        TempData["EditPasswordMessage"] = "Password changed successfully";
                        TempData["type"] = "success";
                    }
                }
                if (!flag)
                {
                    TempData["type"] = "danger";
                }
                return RedirectToAction("EmployeeEditProfile", "User", new { id = UserData.EmployeeID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing employee password");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult EditEmployeeData(Employee employee)
        {
            try
            {
                Employee value = _db.Employees.Find(employee.Id);
                value.Name = employee.Name;
                value.Address = employee.Address;
                value.Email = employee.Email;
                value.ContactNumber = employee.ContactNumber;
                value.Gender = employee.Gender;
                value.EmpType = employee.EmpType;
                value.EmpRole = employee.EmpRole;
                value.EmpStage = employee.EmpStage;
                value.JoinDate = employee.JoinDate;
                value.CurrentStageCompletionDate = employee.CurrentStageCompletionDate;
                if (employee.Password != null)
                {
                    if (employee.Password.Length < 5)
                    {
                        TempData["type"] = "danger";
                        TempData["EditProfileMessage"] = "Password must be atleast 5 character";
                        return RedirectToAction("EmployeeEditProfile", "User", new { id = value.Id });
                    }
                    if (value.Password != employee.Password)
                    {
                        value.Password = PwdEncryption.HashPassword(employee.Password);
                    }
                }
                value.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
                TempData["type"] = "success";
                TempData["EditProfileMessage"] = "Profile Update Successful";
                return RedirectToAction("EmployeeEditProfile", "User", new { id = value.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing employee data");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult redirectPage()
        {
            Employee empData = SessionService.GetSession(HttpContext);
            if (empData.IsAdmin)
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("EmployeeTask", "UserTask");
        }
        public IActionResult CategorizedUserList(string? type, string? subType)
        {
            try
            {
                ViewBag.SubType = subType == null ? "Employee" : subType;
                List<Employee> userList = _db.Employees.Where(x => x.IsDeleted == false).ToList();
                if (type != null && subType != null)
                {
                    if (type == "Stage")
                    {
                        userList = userList.Where(x => x.EmpStage == (subType == "Internship" ? EnumEmployeeStage.Internship : subType == "ProbationPeriod" ? EnumEmployeeStage.Probation_Period : EnumEmployeeStage.Contractual)).ToList();
                    }
                    else if (type == "Role")
                    {
                        userList = userList.Where(x => x.EmpRole == (subType == "Admin" ? EnumMajorRole.Admin : subType == "Developer" ? EnumMajorRole.Developer : EnumMajorRole.CustomerSupport_QA)).ToList();
                    }
                    else if(type == "UserType")
                    { 
                        userList = userList.Where(x => x.EmpType == EnumEmployeeType.Admin).ToList();
                    }
                }

                return View("CategorizedUserList", userList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returing categorized user list");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }

    }
}
