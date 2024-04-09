namespace employeeDailyTaskRecorder.Hangfire
{
    public interface ISendEmailWorker
    {
        Task SendEmailToAdmin();
        Task SendEmailToEmployee();
    }
}
