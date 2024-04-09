using Microsoft.AspNetCore.Mvc;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Models;
using Microsoft.EntityFrameworkCore;
using employeeDailyTaskRecorder.HelperService;
using employeeDailyTaskRecorder.CustomAttributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using employeeDailyTaskRecorder.Mail;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using employeeDailyTaskRecorder.StaticHelper;
using Microsoft.Build.Framework;
using MailKit.Net.Imap;
using MimeKit;
using MailKit;
using Microsoft.IdentityModel.Tokens;
using MailKit.Search;
using Microsoft.Extensions.Logging;
namespace employeeDailyTaskRecorder.Controllers
{
    //[GeneralAuthorization]
    [AdminAuthorization]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private Employee _ActiveUser => SessionService.GetSession(HttpContext);
        public AdminController(ILogger<AdminController> logger, ApplicationDbContext db, IConfiguration configuration, IWebHostEnvironment webHostEnvironment) : base()
        {
            _db = db;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;

        }
        [HttpGet]
        [HttpPost]
        private IReadOnlyList<Employee> _EmpList()
        {
            return _db.Employees
                .AsNoTracking()
                .ToList();
        }
        public async Task<ActionResult> SendEmail()
        {
            try
            {
                int sn = 1;
                var EmpList = _EmpList();
                List<Record> records = _db.Records
                    .Where(x => x.TaskPerformedDate.Date == DateTime.Today.Date)
                    .ToList();
                var task = records;
                StringBuilder tableHtml = new StringBuilder();
                tableHtml.Append("<table border='1' cellspacing='0' cellpadding='10' id='employeeList' style='border-collapse: collapse; width: 100%;'>");
                tableHtml.Append("<thead><tr style='background-color: #37304C; color: #fff; font-weight: bold;'>");
                tableHtml.Append("<th style='text-align: left;'>SN</th>");
                tableHtml.Append("<th style='text-align: left;'>Employee Name</th>");
                tableHtml.Append("<th style='text-align: left;'>Task Description</th>");
                tableHtml.Append("</tr></thead><tbody>");
                foreach (var recEmp in EmpList)
                {
                    var rec = task.FirstOrDefault(x => x.EmployeeId == recEmp.Id);
                    if (rec == null) { rec = new Record(); }
                    tableHtml.Append("<tr>");
                    tableHtml.Append($"<td style='text-align: left;'>{sn++}</td>");
                    tableHtml.Append($"<td style='text-align: left;'>{recEmp.Name}</td>");
                    tableHtml.Append($"<td style='text-align: left;'>{rec.Task}</td>");
                    tableHtml.Append("</tr>");
                }
                tableHtml.Append("</tbody></table>");
                string emailBody = $"<html><body><h2>Today's employee tasks</h2>{tableHtml.ToString()}</body></html>";
                if (task != null)
                {
                    await EmailService.SendEmailAsync(_configuration, emailBody, null, "Today's Employee Tasks");
                }
                return Content("Email sent successfully");
            }
            catch(Exception ex) {
                _logger.LogError(ex, "An error occurred while sending email");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult UploadPdf(IFormFile? pdfFile)
        {
            try
            {
                if (pdfFile != null && pdfFile.ContentType == "application/pdf")
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"Pdf");
                    string tempFilePath = Path.Combine(productPath, "temp_" + fileName);
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        pdfFile.CopyTo(fileStream);
                        var reader = new PdfReader(fileStream);
                        var docuement = new Document();
                        var writer = PdfWriter.GetInstance(docuement, fileStream);
                        docuement.Open();
                        for (var pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                        {
                            var page = writer.GetImportedPage(reader, pageNumber);
                            docuement.Add(iTextSharp.text.Image.GetInstance(page));
                        }
                        docuement.Close();
                        var compressedPdfBytes = fileStream;
                        return File(compressedPdfBytes, "application/pdf", "compressed.pdf");
                    }
                }
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading pdf");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
        public IActionResult Dashboard(int? days)
        {
            try
            {
                ViewBag.CompletionDays = days ?? 15;
                VMAdminIndex Result = new VMAdminIndex();
                Result.TotalEmployee = _db.Employees.Where(x => x.IsDeleted == false).Count();
                Result.TotalIntern = _db.Employees.Where(x => x.EmpStage == EnumEmployeeStage.Internship && x.IsDeleted == false).Count();
                Result.TotalProvisionPeriod = _db.Employees.Where(x => x.EmpStage == EnumEmployeeStage.Probation_Period && x.IsDeleted == false).Count();
                Result.TotalContractual = _db.Employees.Where(x => x.EmpStage == EnumEmployeeStage.Contractual && x.IsDeleted == false).Count();
                Result.TotalAdmin = _db.Employees.Where(x => x.EmpType == EnumEmployeeType.Admin && x.IsDeleted == false).Count();
                Result.TotalDeveloper = _db.Employees.Where(x => x.EmpRole == EnumMajorRole.Developer && x.IsDeleted == false).Count();
                Result.TotalQA = _db.Employees.Where(x => x.EmpRole == EnumMajorRole.CustomerSupport_QA && x.IsDeleted == false).Count();
                Result.TotalMale = _db.Employees.Where(x => x.Gender == EnumEmployeeGender.Male && x.IsDeleted == false).Count();
                Result.TotalFemale = _db.Employees.Where(x => x.Gender == EnumEmployeeGender.Female && x.IsDeleted == false).Count();
                Result.EmployeeList = _db.Employees.Where(x => x.CurrentStageCompletionDate.Date >= DateTime.Now && x.CurrentStageCompletionDate <= DateTime.Now.AddDays(days ?? 15) && x.IsDeleted == false).ToList();
                return View(Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while returning dashboard page");
                return RedirectToAction("TryCatchError", "ErrorHandling");

            }
        }
    }
}
