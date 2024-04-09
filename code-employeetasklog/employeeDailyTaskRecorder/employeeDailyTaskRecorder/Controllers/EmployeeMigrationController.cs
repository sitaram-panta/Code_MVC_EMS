using Microsoft.AspNetCore.Mvc;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Interface;
using employeeDailyTaskRecorder.Models;

namespace employeeDailyTaskRecorder.Controllers
{
    public class EmployeeMigrationController : _BaseMigrationController<IEmployeeMigrationRepo, EmployeeMigration>
    {
        private readonly IConfiguration _configuration;
        private readonly IEmployeeMigrationRepo _employeeMigrationRepo;
        private readonly IWebHostEnvironment _env; // Inject IWebHostEnvironment

        public EmployeeMigrationController(ILogger<AuthController> logger, ApplicationDbContext db, IEmployeeMigrationRepo employeeMigrationRepo, IConfiguration configuration, IWebHostEnvironment env)
            : base(logger,db, employeeMigrationRepo)
        {
            _configuration = configuration;
            _employeeMigrationRepo = employeeMigrationRepo;
            _env = env; // Initialize IWebHostEnvironment
        }
        [HttpGet]
        public override IActionResult Index()
        {
           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async override Task<IActionResult> Index(IFormFile MainFile)
        {
            try
            {
                if (MainFile == null || MainFile.Length == 0)
                {
                    throw new Exception("Invalid File upload size. Please check again.");
                }

                string[] allowedExtensions = new string[] { ".xls", ".xlsx" };
                string extension = Path.GetExtension(MainFile.FileName);

                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("Invalid file format. Please upload a valid Excel file.");
                }

                var webRootPath = _env.WebRootPath;
                var uploadDirectory = Path.Combine(webRootPath, "UploadFile");

                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + MainFile.FileName;
                string filePath = Path.Combine(uploadDirectory, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await MainFile.CopyToAsync(stream);
                }

                _employeeMigrationRepo.RawToDatabase(filePath, "mig_Employees");
                _employeeMigrationRepo.RawDataProcess("mig_Employees");
                var migrationResults = _employeeMigrationRepo.Migrate();

                ViewBag.Message = "Data uploaded and migrated successfully.";

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error while uploading and processing data. Please check the error details for more information: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Other actions...
    }
}
