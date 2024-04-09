using MailKit.Net.Imap;
using MimeKit;
using MailKit;
using employeeDailyTaskRecorder.Data;
using employeeDailyTaskRecorder.Models;
using MailKit.Search;
using System.Text;
using employeeDailyTaskRecorder.Mail;
namespace employeeDailyTaskRecorder.Hangfire
{
    public class ReadEmailForAbsent : IReadEmailForAbsent
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        public ReadEmailForAbsent(ApplicationDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task ReadEmail()
        {
            var leaveRequestSection = _configuration.GetSection("LeaveRequest");
            string[] employeesForLeaveRequest = new string[20];
            int count = 0;
            string host = leaveRequestSection["Host"];
            int port = int.Parse(leaveRequestSection["Port"]);
            string username = leaveRequestSection["UserName"];
            string password = leaveRequestSection["Password"];
            var arrayKeywords = leaveRequestSection.GetSection("Keywords").Get<List<string>>();
            var arrayLeaveDay = leaveRequestSection.GetSection("leaveDay").Get<List<string>>();
            using (var client = new ImapClient())
            {
                client.Connect(host, port, true);

                client.Authenticate(username, password);
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);
                DateTime currentDate = DateTime.Now.Date;
                var unseenMessages = inbox.Search(
                 SearchQuery.NotFlagged
                     .And(SearchQuery.SentSince(currentDate)));
                foreach (var messageUniqueId in unseenMessages)
                {
                    var fullMessage = inbox.GetMessage(messageUniqueId);
                    var subject = fullMessage.Subject;
                    var from = fullMessage.From;
                    var mailAddress = new System.Net.Mail.MailAddress(from.ToString());
                    var emailAddress = mailAddress.Address;
                    var body = !string.IsNullOrEmpty(fullMessage.TextBody) ? fullMessage.TextBody : fullMessage.HtmlBody;

                    var employee = _db.Employees.FirstOrDefault(x => x.Email == emailAddress);

                    if (employee != null)
                    {
                        if (ContainsWord(body, arrayKeywords) != null || ContainsWord(subject, arrayKeywords) != null)
                        {
                            var leaveRequest = new LeaveRequest
                            {
                                EmployeeId = employee.Id,
                                RequestDate = fullMessage.Date.UtcDateTime,
                                LeaveRemarks = body
                            };
                            employeesForLeaveRequest[count] = employee.Name;
                            count++;
                            if (ContainsWord(body, arrayLeaveDay) == "tommorrow")
                            {
                                leaveRequest.FromDate = DateTime.Now.AddDays(1);
                                leaveRequest.ToDate = DateTime.Now.AddDays(1);
                            }
                            else
                            {
                                leaveRequest.FromDate = DateTime.Now;
                                leaveRequest.ToDate = DateTime.Now;
                            }

                            _db.leaveRequest.Add(leaveRequest);
                            _db.SaveChanges();


                            inbox.SetFlags(messageUniqueId, MessageFlags.Flagged, true);
                        }
                    }
                }

                client.Disconnect(true);
                if (count != 0)
                {
                    sendEmailToAdmin(employeesForLeaveRequest, count);
                }
            }
        }
        public static string ContainsWord(string sentence, List<string> keywords)
        {
            foreach (var word in keywords)
            {
                if (sentence.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0) return word.ToLower();
            }
            return null;
        }
        public async void sendEmailToAdmin(string[] employeeList, int count)
        {
            StringBuilder adminEmail = new StringBuilder();
            adminEmail.Append($"<p style='margin-bottom:10px;'>Dear Admin</p>");
            adminEmail.Append($"<p>We would like to inform you that we have received new leave requests for {count} employees:</p>");
            for (int i = 0; i < count; i++)
            {
                adminEmail.Append("<ul>");
                adminEmail.Append($"<li>{employeeList[i]}</li>");
                adminEmail.Append("</ul>");
            }
            adminEmail.Append($"<p style='margin-top:10px;'>Best regards</p>");
            adminEmail.Append($"<p style='margin-top:5px;'>Task Management System</p>");
            string emailBody = $"<html><body><h2>Today's employee tasks</h2>{adminEmail.ToString()}</body></html>";
            await EmailService.SendEmailAsync(_configuration, emailBody, "gcanjit@gmail.com", "New Leave Request");

        }

    }
}
