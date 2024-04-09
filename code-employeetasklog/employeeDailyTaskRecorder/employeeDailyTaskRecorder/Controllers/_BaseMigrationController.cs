using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Interface;
using employeeDailyTaskRecorder.Migrations;
using System.Collections.Generic;

namespace employeeDailyTaskRecorder.Controllers
{
    public abstract class _BaseMigrationController<MainRepo, MigEntity> : AuthController
   where MainRepo : _IBaseMigrationRepo<MigEntity>
   where MigEntity : class
    {
        private readonly MainRepo _MigrationRepo;
        private readonly string _RawTableName;

        public _BaseMigrationController(ILogger<AuthController> logger, ApplicationDbContext db, MainRepo MigrationRepo)
            : base(logger,db)
        {
            _MigrationRepo = MigrationRepo;
            _RawTableName = typeof(MigEntity).Name;
        }

        // GET: _BaseMigration
        [HttpGet]
        public new virtual IActionResult Index()
        {
            return View();
        }

        
       
        [ValidateAntiForgeryToken]
        [HttpPost]
        public  virtual async Task<IActionResult> Index(IFormFile MainFile)
        {
            try
            {
                if (MainFile == null || MainFile.Length == 0)
                {
                    throw new Exception("Invalid File upload size. Please check again.");
                }

                string[] exts = new string[] { ".xls", ".xlsx" };
                string upFileName = _RawTableName + DateTime.UtcNow.ToString("yyMMddhhmmss") + ".xlsx";
                UploadResponse res =  UploadHelper.UploadFile(MainFile, "~/UploadFile/" + _RawTableName, exts, upFileName);
                if (!res.Status)
                {
                    throw new Exception("Error while uploading file. Error Detail: " + res.ErrorMessage);
                }
                 _MigrationRepo._db.Database.ExecuteSqlRaw("TRUNCATE TABLE [" + _RawTableName + "]");
                _MigrationRepo.RawToDatabase(Path.Combine(Directory.GetCurrentDirectory(), res.VirtualFullPath), _RawTableName);
                _MigrationRepo.RawDataProcess(_RawTableName);
                // You need to handle success and error cases, e.g., with TempData
                TempData["Message"] =  "Data uploaded successfully to: " + _RawTableName;
                return RedirectToAction("UploadedInformation");

            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g., with TempData
                TempData["Error"] = "Error while uploading and processing data. Please check the error details for more information. Error: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        public virtual async Task<IActionResult> UploadedInformation()
        {
            IList<MigEntity> DataList = (IList<MigEntity>)await _MigrationRepo.RawDataList().ToListAsync();
            return View(DataList);
        }

        public virtual IActionResult MigrateToTable()
        {
            try
            {
                IDictionary<MigEntity, string> FailedList = (IDictionary< MigEntity, string>)_MigrationRepo.Migrate();
                if (FailedList.Count > 1)
                {
                    // Handle the error case, e.g., with TempData
                    TempData["Error"] = "Migrated with error:: " + string.Join(", ", FailedList.Values.Take(10));
                }
                else
                {
                    // Handle the success case, e.g., with TempData
                    TempData["Message"] = "Successfully migrated.";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Handle the exception here, e.g., with TempData
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("UploadedInformation");
        }
    }
}
