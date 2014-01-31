using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OSBIDE.Web.Models.Attributes
{
    public class OsbideAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Authentication auth = new Authentication();
            string key = auth.GetAuthenticationKey();

            //were we supplied an authentication key from the query string?
            if (filterContext.HttpContext != null)
            {
                string authQueryKey = filterContext.HttpContext.Request.QueryString.Get("auth");
                if (authQueryKey != null && authQueryKey.Length > 0)
                {
                    key = authQueryKey;

                    //if the key is valid, log the user into the system and then retry the request
                    if (auth.IsValidKey(key) == true)
                    {
                        auth.LogIn(auth.GetActiveUser(key));
                        RouteValueDictionary routeValues = new RouteValueDictionary();
                        routeValues["controller"] = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                        routeValues["action"] = filterContext.ActionDescriptor.ActionName;
                        foreach (string parameterKey in filterContext.ActionParameters.Keys)
                        {
                            object parameterValue = filterContext.ActionParameters[parameterKey];
                            routeValues[parameterKey] = parameterValue;
                        }
                        filterContext.Result = new RedirectToRouteResult(routeValues);
                        return;
                    }
                }
            }
            if (auth.IsValidKey(key) == false)
            {

                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "Login", returnUrl = filterContext.HttpContext.Request.Url }));
            }
            else
            {
                //log the request
                ActionRequestLog log = new ActionRequestLog();
                log.ActionName = filterContext.ActionDescriptor.ActionName;
                log.ControllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                log.CreatorId = auth.GetActiveUser(key).Id;
                try
                {
                    log.IpAddress = filterContext.RequestContext.HttpContext.Request.ServerVariables["REMOTE_ADDR"];
                }
                catch(Exception)
                {
                    log.IpAddress = "Unknown";
                }
                StringBuilder parameters = new StringBuilder();
                foreach (string parameterKey in filterContext.ActionParameters.Keys)
                {
                    object parameterValue = filterContext.ActionParameters[parameterKey];
                    if (parameterValue == null)
                    {
                        parameterValue = ActionRequestLog.ACTION_PARAMETER_NULL_VALUE;
                    }
                    parameters.Append(string.Format("{0}={1}{2}", parameterKey, parameterValue.ToString(), ActionRequestLog.ACTION_PARAMETER_DELIMITER));
                }
                log.ActionParameters = parameters.ToString();

                //save to DB
                using (OsbideContext db = OsbideContext.DefaultWebConnection)
                {
                    db.ActionRequestLogs.Add(log);
                    db.SaveChanges();
                }
            }
        }
    }
}