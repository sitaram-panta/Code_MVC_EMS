using employeeDailyTaskRecorder.Data;
using Microsoft.EntityFrameworkCore;
using employeeDailyTaskRecorder.Seed;
using Hangfire;
using Hangfire.SqlServer;
using employeeDailyTaskRecorder.Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Mvc.Filters;
using employeeDailyTaskRecorder.CustomAttributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using Serilog;
using employeeDailyTaskRecorder.Interface;
using employeeDailyTaskRecorder.Repository;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
         .ReadFrom.Configuration(builder.Configuration)
         .Enrich.FromLogContext()
         .CreateLogger();
builder.Host.UseSerilog(Log.Logger);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
});

builder.Services.AddHangfire(configuration => configuration
.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
.UseSimpleAssemblyNameTypeSerializer()
.UseRecommendedSerializerSettings()
.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddTransient<ISendEmailWorker, SendEmailWorker>();
builder.Services.AddTransient<IReadEmailForAbsent, ReadEmailForAbsent>();
builder.Services.AddScoped<IEmployeeMigrationRepo, EmployeeMigrationRepo>();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    seedData.Initialize(context);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
 pattern: "{controller=Auth}/{action=Index}/{id?}");
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = new[] { new DashboardAuthorizationFilter() }
    //IsReadOnlyFunc = (DashboardContext context) => true
});

//app.MapHangfireDashboard();
TimeZoneInfo serverTimeZone = TimeZoneInfo.Local;
DateTime serverLocalTimeForAdmin = TimeZoneInfo.ConvertTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 0, 0), serverTimeZone);
DateTime serverLocalTimeForEmp = TimeZoneInfo.ConvertTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 0, 0), serverTimeZone);

// Register your background job(s) using the DI container
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var backgroundJobClass = services.GetRequiredService<ISendEmailWorker>();

    // Add a recurring job using the DI-injected instance
    RecurringJob.AddOrUpdate(() => backgroundJobClass.SendEmailToAdmin(), Cron.Daily(serverLocalTimeForAdmin.Hour), serverTimeZone);
    RecurringJob.AddOrUpdate(() => backgroundJobClass.SendEmailToEmployee(), Cron.Daily(serverLocalTimeForEmp.Hour), serverTimeZone);
    RecurringJob.AddOrUpdate(() => services.GetRequiredService<IReadEmailForAbsent>().ReadEmail(), Cron.MinuteInterval(5), serverTimeZone);

}
app.Run();


