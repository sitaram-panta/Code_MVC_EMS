namespace employeeDailyTaskRecorder.StaticHelper
{
    public static class ViewHelper
    {
        public static string GetDate(DateTime d) => d.ToString("yyyy/MM/dd");
        public static string GetDate(DateTime? d) => d.HasValue ? GetDate(d.Value) : "";
    }
    public static class DatetimeExtension
    {
        public static string ShowTime(this DateTime d) => d.ToString("HH:mm");
        public static string ShowDate(this DateTime d) => d.ToString("yyyy/MM/dd");
    }
}
