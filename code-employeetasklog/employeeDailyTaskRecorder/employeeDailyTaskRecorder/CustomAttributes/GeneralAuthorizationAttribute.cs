﻿using employeeDailyTaskRecorder.HelperService;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace employeeDailyTaskRecorder.CustomAttributes
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var user = SessionService.GetSession(context.GetHttpContext());
            return user != null;
            //var httpContext = context.GetHttpContext();

            //// Allow all authenticated users to see the Dashboard (potentially dangerous).
            //return httpContext.User.Identity?.IsAuthenticated ?? false;
        }
    }
    public class GeneralAuthorizationAttribute : Attribute, IAuthorizationFilter
    {

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = SessionService.GetSession(context.HttpContext);
            bool isLogged = user != null;
            if (!isLogged)
            {
                context.Result = new RedirectResult("/Auth/Index");
                return;
            }

        }

    }
    public class AdminAuthorizationAttribute : GeneralAuthorizationAttribute, IAuthorizationFilter
    {
        public new void OnAuthorization(AuthorizationFilterContext context)
        {
            base.OnAuthorization(context);
            var user = SessionService.GetSession(context.HttpContext);
            if ((user != null && user.IsUser))
            {
                context.Result = new RedirectResult("/ErrorHandling/Index");
                return;
            }


        }
    }
    public class UserAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        public new void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = SessionService.GetSession(context.HttpContext);
            if (user != null && user.IsAdmin)
            {
                context.Result = new RedirectResult("/ErrorHandling/Index");
                return;
            }
        }
    }
}
