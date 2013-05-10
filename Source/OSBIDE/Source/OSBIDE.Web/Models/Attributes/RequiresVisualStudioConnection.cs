﻿using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OSBIDE.Web.Models.Attributes
{
    /// <summary>
    /// Using this attribute requires student-type users to have an active connection to visual studio.
    /// </summary>
    public class RequiresVisualStudioConnectionForStudents : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Authentication auth = new Authentication();
            string key = auth.GetAuthenticationKey();
            OsbideUser user = auth.GetActiveUser(key);

            //is the user a student?
            if (user.Role == SystemRole.Student)
            {
                //only allow access if they've been active in Visual Studio in the last 10 minutes
                if (user.LastVsActivity > DateTime.Now.Subtract(new TimeSpan(0, 0, 10, 0, 0)))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "RequiresActiveVsConnection" }));
                }
            }
        }
    }
}