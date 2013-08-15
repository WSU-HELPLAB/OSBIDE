using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace OSBIDE.Web.Models.Queries
{
    public class ActivityFeedQuery : IOsbideQuery<FeedItem>
    {
        protected OsbideContext _db;
        protected List<IOsbideEvent> eventSelectors = new List<IOsbideEvent>();
        protected List<OsbideUser> subscriptionSubjects = new List<OsbideUser>();
        protected List<int> eventIds = new List<int>();
        protected StringBuilder _query_base_select = new StringBuilder(@"SELECT 
                                  log.Id AS log_id
                                , log.LogType
                                , log.DateReceived
                                , log.SenderId
                                , log.AssemblyVersion
                                , score.*
                                , osbideUser.Id AS user_Id
                                , osbideUser.Email
                                , osbideUser.FirstName
                                , osbideUser.LastName
                                , osbideUser.SchoolId
                                , osbideUser.InstitutionId
                                , osbideUser.RoleValue
                                , osbideUser.LastVsActivity
                                ");
        protected StringBuilder _query_additional_select = new StringBuilder();
        protected StringBuilder _query_from_clause = new StringBuilder(@"
                                FROM EventLogs log
                                LEFT JOIN UserScores score ON log.SenderId = score.UserId
                                LEFT JOIN OsbideUsers osbideUser ON log.SenderId = osbideUser.Id
                                ");
        protected StringBuilder _query_joins = new StringBuilder();
        protected StringBuilder _query_where_clause = new StringBuilder("WHERE 1 = 1\n");
        protected StringBuilder _query_order_by = new StringBuilder(@"
                                ORDER BY
                                    log.DateReceived DESC
                                ");
        protected StringBuilder _query_limit = new StringBuilder();

        public ActivityFeedQuery(OsbideContext db)
        {
            if (db == null)
            {
                throw new Exception("db parameter cannot be null");
            }
            _db = db;

            StartDate = DateTime.MinValue;
            EndDate = DateTime.UtcNow.AddDays(1.0);
            MinLogId = -1;
            MaxLogId = -1;
            MaxQuerySize = -1;
        }

        /// <summary>
        /// Sets a limit on the newest post to be retrieved.  Example: if <see cref="EndDate"/> is set to
        /// 2010-01-01, no posts after 2010-01-01 will be retrieved.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Sets a limit on the oldest post to be retrieved.  Example: if <see cref="StartDate"/> is set to
        /// 2010-01-01, no posts before 2010-01-01 will be retrieved.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Used to set a floor on the logs to retrieve.  Example: if <see cref="MinLogId"/> is set to 5,
        /// no posts with an Id less than 6 will be retrieved.
        /// </summary>
        public int MinLogId { get; set; }

        /// <summary>
        /// Used to set a ceiling on the logs to retrieve.  Example: if <see cref="MaxLogId"/> is set to 5,
        /// no posts with an Id greater than 4 will be retrieved.
        /// </summary>
        public int MaxLogId { get; set; }

        /// <summary>
        /// Used to limit the number of query results.  Default of -1 means to return all results.
        /// </summary>
        public int MaxQuerySize { get; set; }

        //returns a list of all possible events that a user can subscribe to
        public static List<IOsbideEvent> GetAllEvents()
        {
            List<IOsbideEvent> events = new List<IOsbideEvent>();
            events.Add(new BuildEvent());
            events.Add(new ExceptionEvent());
            events.Add(new FeedPostEvent());
            events.Add(new AskForHelpEvent());
            events.Add(new LogCommentEvent());
            events.Add(new HelpfulMarkGivenEvent());
            
            //AC: turned off for fall 2013 study
            //events.Add(new SubmitEvent());
            return events;
        }

        public void AddEventType(IOsbideEvent evt)
        {
            if (eventSelectors.Where(e => e.EventName.CompareTo(evt.EventName) == 0).Count() == 0)
            {
                eventSelectors.Add(evt);
            }
        }

        public List<IOsbideEvent> ActiveEvents
        {
            get
            {
                return eventSelectors.ToList();
            }
        }

        public void AddSubscriptionSubject(OsbideUser user)
        {
            if (user != null)
            {
                subscriptionSubjects.Add(user);   
            }
        }

        public void AddSubscriptionSubject(IEnumerable<OsbideUser> users)
        {
            subscriptionSubjects = subscriptionSubjects.Union(users).ToList();
        }

        public void ClearSubscriptionSubjects()
        {
            subscriptionSubjects = new List<OsbideUser>();
        }

        public void AddEventId(int id)
        {
            eventIds.Add(id);
        }

        public virtual IList<FeedItem> Execute()
        {
            List<FeedItem> feedItems = new List<FeedItem>();
            StringBuilder queryString = new StringBuilder();
            Dictionary<string, IOsbideEvent> aliasMap = new Dictionary<string, IOsbideEvent>();

            //force adding event types when querying specific log ids allows the joins to take place
            //where they otherwise might.  The joins are needed in order to properly build the objects
            if (eventIds.Count > 0)
            {
                var eventTypesQuery = (from evt in _db.EventLogs.AsNoTracking()
                                       where eventIds.Contains(evt.Id)
                                       select evt).ToList();
                foreach (EventLog log in eventTypesQuery)
                {
                    AddEventType(EventFactory.FromName(log.LogType));
                }
            }

            //make the joins
            string tablePrefix = "T";
            int joinCounter = 1;
            foreach (IOsbideEvent evt in eventSelectors)
            {
                string alias = string.Format("{0}{1}", tablePrefix, joinCounter);
                aliasMap[alias] = evt;
                string tableName = string.Format("{0}s", evt.EventName);
                _query_additional_select.Append(string.Format(", {0}.*, {1}.Id AS {2}_Id\n", alias, alias, alias));
                _query_joins.Append(string.Format(@"
                        LEFT JOIN {0} {1} ON log.Id = {2}.EventLogId
                    ", tableName
                     , alias
                     , alias
                     ));
                joinCounter++;
            }

            //were we supplied with an ending date?
            if(EndDate < DateTime.MaxValue)
            {
                _query_where_clause.Append(string.Format(" AND log.DateReceived < '{0}'\n", EndDate.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            //were we supplied with an starting date?
            if (StartDate > DateTime.MinValue)
            {
                _query_where_clause.Append(string.Format(" AND log.DateReceived > '{0}'\n", StartDate.ToString("yyyy-MM-dd HH:mm:ss")));
            }

            //were we supplied with a minimum log Id number?
            if (MinLogId > 0)
            {
                _query_where_clause.Append(string.Format(" AND log.Id > {0}\n", MinLogId));
            }

            //were we supplied with a maximum ID number?
            if (MaxLogId > 0)
            {
                _query_where_clause.Append(string.Format(" AND log.Id < {0}\n", MaxLogId));
            }

            //if we were asked to retrieve a certain list of events, add that into the query
            if (eventIds.Count > 0)
            {
                _query_where_clause.Append(string.Format(" AND log.Id IN ({0})\n", string.Join(",", eventIds)));
            }

            //filter by desired events if desired
            string[] eventNames = eventSelectors.Select(e => e.EventName).ToArray();
            if (eventNames.Length > 0)
            {
                //sanitize strings for query
                for (int i = 0; i < eventNames.Length; i++)
                {
                    eventNames[i] = string.Format("'{0}'", eventNames[i]);
                }
                _query_where_clause.Append(string.Format(" AND log.LogType IN ({0})\n", string.Join(",", eventNames)));
            }

            //get list of subscription ids
            int[] subjectIds = subscriptionSubjects.Select(s => s.Id).ToArray();
            if (subjectIds.Length > 0)
            {
                _query_where_clause.Append(string.Format(" AND log.SenderId IN ({0})\n", string.Join(",", subjectIds)));
            }

            //did we only want a certain number of results returned?
            if (MaxQuerySize > 0)
            {
                _query_limit.Clear();
                _query_limit.Append(string.Format("OFFSET 0 ROWS\n FETCH NEXT {0} ROWS ONLY", MaxQuerySize)); 
            }

            //build the query
            queryString.Append(_query_base_select);
            queryString.Append(_query_additional_select);
            queryString.Append(_query_from_clause);
            queryString.Append(_query_joins);
            queryString.Append(_query_where_clause);
            queryString.Append(_query_order_by);
            queryString.Append(_query_limit);

            SqlConnection conn = new SqlConnection(ControllerBase.DefaultConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString.ToString(), conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    //read values into a dictionary for easier processing
                    Dictionary<string, object> values = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string index = reader.GetName(i);
                        if (reader[i] != null && string.IsNullOrEmpty(reader[i].ToString()) == false)
                        {
                            values[index] = reader[i];
                        }
                    }

                    //find the event that isn't null
                    IOsbideEvent nonNullEvent = null;
                    foreach (KeyValuePair<string, IOsbideEvent> pair in aliasMap)
                    {
                        string dbKey = string.Format("{0}_Id", pair.Key);
                        if (reader[dbKey] != null && string.IsNullOrEmpty(reader[dbKey].ToString()) == false)
                        {
                            nonNullEvent = pair.Value.FromDict(values);
                            break;
                        }
                    }
                    if (nonNullEvent != null)
                    {
                        FeedItem item = new FeedItem();
                        item.Event = nonNullEvent;
                        item.EventId = nonNullEvent.Id;
                        item.LogId = (int)reader[0];
                        item.Log = new EventLog()
                        {
                            Id = (int)reader[0],
                            LogType = reader["LogType"].ToString(),
                            DateReceived = (DateTime)reader["DateReceived"],
                            SenderId = (int)reader["SenderId"],
                            Sender = new OsbideUser()
                            {
                                Email = reader["Email"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                Id = (int)reader["user_Id"],
                                InstitutionId = (int)reader["institutionId"],
                                LastName = reader["LastName"].ToString(),
                                LastVsActivity = (DateTime)reader["LastVsActivity"],
                                Role = (SystemRole)reader["RoleValue"],
                                SchoolId = (int)reader["SchoolId"]
                            },
                            AssemblyVersion = reader["AssemblyVersion"].ToString()
                        };
                        feedItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                ServerLog log = new ServerLog();
                log.LogMessage = ex.Message;
                _db.ServerLogs.Add(log);
                _db.SaveChanges();
                return feedItems;
            }
            finally
            {
                conn.Close();
            }

            //update comments and helpful marks that will appear in the activity feed (as opposed to being attached to another log)
            foreach (FeedItem item in feedItems)
            {
                if (item.Event is LogCommentEvent)
                {
                    item.Event = _db.LogCommentEvents.Find(item.EventId);
                }
                else if (item.Event is HelpfulMarkGivenEvent)
                {
                    item.Event = _db.HelpfulMarkGivenEvents.Find(item.EventId);
                }
            }

            //pull comments for all feed items
            Dictionary<int, FeedItem> itemsDict = new Dictionary<int, FeedItem>();
            foreach(FeedItem item in feedItems)
            {
                itemsDict[item.LogId] = item;
            }

            int[] logIds = itemsDict.Keys.ToArray();
            var commentsQuery = from comment in _db.LogCommentEvents
                                where logIds.Contains(comment.SourceEventLogId)
                                select comment;
            List<LogCommentEvent> comments = commentsQuery.ToList();
            foreach (LogCommentEvent comment in comments)
            {
                itemsDict[comment.SourceEventLogId].Comments.Add(comment);
                itemsDict[comment.SourceEventLogId].Log.Comments.Add(comment);
                itemsDict[comment.SourceEventLogId].HelpfulComments += comment.HelpfulMarks.Count;
            }
            return feedItems;
        }
    }
}