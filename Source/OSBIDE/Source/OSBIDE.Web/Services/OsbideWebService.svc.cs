using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using OSBIDE.Library.Models;
using System.Data.Entity;
using System.Threading;
using OSBIDE.Library.Events;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models;
using OSBIDE.Library;
using System.Text.RegularExpressions;

namespace OSBIDE.Web.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OsbideWebService
    {
        public OsbideContext Db { get; private set; }

        public OsbideWebService()
        {
#if DEBUG
            Db = new OsbideContext("OsbideDebugContext");
            Database.SetInitializer<OsbideContext>(new OsbideContextModelChangeInitializer());
#else
            Db = new OsbideContext("OsbideReleaseContext");
#endif
        }

        [OperationContract]
        public string Echo(string toEcho)
        {
            return toEcho;
        }

        /// <summary>
        /// Attempts to log the user into OSBIDE.  Returns a hash that can be used to authenticate future requests.  
        /// If the login fails, the hash will be an empty string.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>A hashed string that can be used to authenticate future requests</returns>
        [OperationContract]
        public string Login(string email, string hashedPassword)
        {
            string hash = "";
            if (UserPassword.ValidateUserHashedPassword(email, hashedPassword, Db))
            {
                Authentication auth = new Authentication();
                OsbideUser user = Db.Users.Where(u => u.Email.CompareTo(email) == 0).FirstOrDefault();
                if (user != null)
                {
                    hash = auth.LogIn(user);
                }
            }
            return hash;
        }

        /// <summary>
        /// Will return the user identified by the supplied authentication token
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public OsbideUser GetActiveUser(string authToken)
        {
            Authentication auth = new Authentication();
            return new OsbideUser(auth.GetActiveUser(authToken));
        }

        /// <summary>
        /// Tells the client whether or not the supplied token is valid.  May be used as a
        /// "keep alive" method.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public bool IsValidKey(string authToken)
        {
            Authentication auth = new Authentication();
            return auth.IsValidKey(authToken);
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public int SubmitLocalErrorLog(LocalErrorLog errorLog, string authToken)
        {
            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == true)
            {
                OsbideUser authUser = GetActiveUser(authToken);

                //replace the error log's sender information with what we obtained from the auth key
                errorLog.Sender = null;
                errorLog.SenderId = authUser.Id;

                //add to the db and give it a try
                Db.LocalErrorLogs.Add(errorLog);
                try
                {
                    Db.SaveChanges();
                }
                catch (Exception)
                {
                    return (int)Enums.ServiceCode.DatabaseError;
                }
                return errorLog.Id;
            }
            else
            {
                return (int)Enums.ServiceCode.AuthenticationError;
            }
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public int SubmitLog(EventLog log, string authToken)
        {

            //verify request before continuing
            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == false)
            {
                return (int)Enums.ServiceCode.AuthenticationError;
            }

            //AC: kind of hackish, but event logs that we receive should already have an ID
            //attached to them from being stored in the machine's local DB.  We can use 
            //that ID to track the success/failure of asynchronous calls.
            int localId = log.Id;

            //we don't want the local id, so be sure to clear
            log.Id = 0;

            //replace sender information with what is contained in the auth key
            OsbideUser authUser = GetActiveUser(authToken);
            log.Sender = null;
            log.SenderId = authUser.Id;

            //insert into the DB
            Db.EventLogs.Add(log);
            try
            {
                Db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                return (int)Enums.ServiceCode.DatabaseError;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation(ex.Message);
                return (int)Enums.ServiceCode.DatabaseError;
            }

            //Tease apart log information and insert into the appropriate DB table
            IOsbideEvent evt = null;
            try
            {
                evt = EventFactory.FromZippedBinary(log.Data.BinaryData, new OsbideDeserializationBinder());
                evt.EventLogId = log.Id;
            }
            catch (Exception)
            {
                return (int)Enums.ServiceCode.DatabaseError;
            }
            if (log.LogType == AskForHelpEvent.Name)
            {
                Db.AskForHelpEvents.Add((AskForHelpEvent)evt);
            }
            else if (log.LogType == BuildEvent.Name)
            {
                BuildEvent build = (BuildEvent)evt;
                Db.BuildEvents.Add(build);

                string pattern = "error ([^:]+)";

                //log all errors in their own DB for faster search
                List<string> errors = new List<string>();
                Dictionary<string, ErrorType> errorTypes = new Dictionary<string, ErrorType>();
                foreach (BuildEventErrorListItem item in build.ErrorItems)
                {
                    Match match = Regex.Match(item.ErrorListItem.Description, pattern);

                    //ignore bad matches
                    if (match.Groups.Count == 2)
                    {
                        string errorCode = match.Groups[1].Value.ToLower().Trim();
                        ErrorType type = Db.ErrorTypes.Where(t => t.Name == errorCode).FirstOrDefault();
                        if(type == null)
                        {
                            if (errorTypes.ContainsKey(errorCode) == false)
                            {
                                type = new ErrorType()
                                {
                                    Name = errorCode
                                };
                                Db.ErrorTypes.Add(type);
                            }
                            else
                            {
                                type = errorTypes[errorCode];
                            }
                        }
                        if (errorCode.Length > 0 && errors.Contains(errorCode) == false)
                        {
                            errors.Add(errorCode);
                        }
                        errorTypes[errorCode] = type;
                    }
                }
                Db.SaveChanges();
                foreach (string errorType in errors)
                {
                    Db.BuildErrors.Add(new BuildError()
                    {
                        BuildErrorTypeId = errorTypes[errorType].Id,
                        LogId = log.Id
                    });
                }

            }
            else if (log.LogType == CutCopyPasteEvent.Name)
            {
                Db.CutCopyPasteEvents.Add((CutCopyPasteEvent)evt);
            }
            else if (log.LogType == DebugEvent.Name)
            {
                Db.DebugEvents.Add((DebugEvent)evt);
            }
            else if (log.LogType == EditorActivityEvent.Name)
            {
                Db.EditorActivityEvents.Add((EditorActivityEvent)evt);
            }
            else if (log.LogType == ExceptionEvent.Name)
            {
                Db.ExceptionEvents.Add((ExceptionEvent)evt);
            }
            else if (log.LogType == FeedCommentEvent.Name)
            {
                Db.FeedCommentEvents.Add((FeedCommentEvent)evt);
            }
            else if (log.LogType == SaveEvent.Name)
            {
                Db.SaveEvents.Add((SaveEvent)evt);
            }
            else if (log.LogType == SubmitEvent.Name)
            {
                Db.SubmitEvents.Add((SubmitEvent)evt);
            }
            try
            {
                Db.SaveChanges();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                return (int)Enums.ServiceCode.DatabaseError;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceInformation(ex.Message);
                return (int)Enums.ServiceCode.DatabaseError;
            }

            //Return the ID number of the local object so that the caller knows that it's been successfully
            //saved into the main system.
            return localId;
        }

        [OperationContract]
        public string LibraryVersionNumber()
        {
            return OSBIDE.Library.StringConstants.LibraryVersion;
        }

        [OperationContract]
        public string OsbidePackageUrl()
        {
            return OSBIDE.Library.StringConstants.OsbidePackageUrl;
        }
    }
}
