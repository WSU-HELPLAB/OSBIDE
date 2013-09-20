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
                                , buildErrors.NumberOfBuildErrors
                                ");
        protected StringBuilder _query_additional_select = new StringBuilder();
        protected StringBuilder _query_from_clause = new StringBuilder(@"
                                FROM EventLogs log
                                LEFT JOIN (
								   SELECT COUNT(BuildErrorTypeId) AS [NumberOfBuildErrors], LogId FROM BuildErrors GROUP BY LogId
								) buildErrors ON log.Id = buildErrors.LogId
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

        /// <summary>
        /// returns a lits of all social events in OSBLE
        /// </summary>
        /// <returns></returns>
        public static List<IOsbideEvent> GetSocialEvents()
        {
            List<IOsbideEvent> events = new List<IOsbideEvent>();
            events.Add(new FeedPostEvent());
            events.Add(new AskForHelpEvent());
            events.Add(new LogCommentEvent());
            events.Add(new HelpfulMarkGivenEvent());

            //AC: turned off for fall 2013 study
            //events.Add(new SubmitEvent());
            return events;
        }

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
            SortedDictionary<DateTime, FeedItem> feedItems = new SortedDictionary<DateTime, FeedItem>();
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

            //were we supplied with an ending date?
            if (EndDate < DateTime.MaxValue)
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
            string[] eventNames = eventSelectors.Where(e => e.EventName != BuildEvent.Name).Select(e => e.EventName).ToArray();
            bool isLookingForBuildError = false;
            if (eventSelectors.Where(e => e.EventName == BuildEvent.Name).Count() > 0)
            {
                isLookingForBuildError = true;
                if (eventNames.Length == 0)
                {
                    eventNames = new string[] { "NULL" };
                }
            }
            if (eventNames.Length > 0)
            {
                //sanitize strings for query
                for (int i = 0; i < eventNames.Length; i++)
                {
                    eventNames[i] = string.Format("'{0}'", eventNames[i]);
                }
                if (isLookingForBuildError == true)
                {
                    _query_where_clause.Append(string.Format(@"AND
                    (
                        log.LogType IN ({0}) OR (log.LogType = '{1}' AND [NumberOfBuildErrors] > 0)
                    )", string.Join(",", eventNames), BuildEvent.Name));

                }
                else
                {
                    _query_where_clause.Append(string.Format(" AND log.LogType IN ({0})\n", string.Join(",", eventNames)));
                }
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

            Dictionary<string, List<int>> dbItems = new Dictionary<string, List<int>>();
            SqlConnection conn = new SqlConnection(ControllerBase.DefaultConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString.ToString(), conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    string logType = reader["LogType"].ToString();
                    if (dbItems.ContainsKey(logType) == false)
                    {
                        dbItems.Add(logType, new List<int>());
                    }
                    dbItems[logType].Add((int)reader["log_id"]);
                }
            }
            catch (Exception ex)
            {
                ServerLog log = new ServerLog();
                log.LogMessage = ex.Message;
                _db.ServerLogs.Add(log);
                _db.SaveChanges();
                return feedItems.Values.ToList();
            }
            finally
            {
                conn.Close();
            }

            foreach (string key in dbItems.Keys)
            {
                List<IOsbideEvent> events = new List<IOsbideEvent>();
                int[] dbKeys = dbItems[key].ToArray();
                if (key.CompareTo(BuildEvent.Name) == 0)
                {
                    events = (from evt in _db.BuildEvents
                                .Include("EventLog")
                                .Include("EventLog.Sender")
                                .Include("EventLog.Comments")
                                where dbKeys.Contains(evt.EventLogId)
                                select evt).ToList().Cast<IOsbideEvent>().ToList();
                }
                else if (key.CompareTo(ExceptionEvent.Name) == 0)
                {
                    events = (from evt in _db.ExceptionEvents
                                .Include("EventLog")
                                .Include("EventLog.Sender")
                                .Include("EventLog.Comments")
                              where dbKeys.Contains(evt.EventLogId)
                              select evt).ToList().Cast<IOsbideEvent>().ToList();
                }
                else if (key.CompareTo(FeedPostEvent.Name) == 0)
                {
                    events = (from evt in _db.FeedPostEvents
                                .Include("EventLog")
                                .Include("EventLog.Sender")
                                .Include("EventLog.Comments")
                              where dbKeys.Contains(evt.EventLogId)
                              select evt).ToList().Cast<IOsbideEvent>().ToList();
                }
                else if (key.CompareTo(AskForHelpEvent.Name) == 0)
                {
                    events = (from evt in _db.AskForHelpEvents
                                .Include("EventLog")
                                .Include("EventLog.Sender")
                                .Include("EventLog.Comments")
                              where dbKeys.Contains(evt.EventLogId)
                              select evt).ToList().Cast<IOsbideEvent>().ToList();
                }
                else if (key.CompareTo(LogCommentEvent.Name) == 0)
                {
                    events = (from evt in _db.LogCommentEvents
                                .Include("EventLog")
                                .Include("EventLog.Sender")
                                .Include("EventLog.Comments")
                              where dbKeys.Contains(evt.EventLogId)
                              select evt).ToList().Cast<IOsbideEvent>().ToList();
                }
                else if (key.CompareTo(HelpfulMarkGivenEvent.Name) == 0)
                {
                    events = (from evt in _db.HelpfulMarkGivenEvents
                                .Include("EventLog")
                                .Include("EventLog.Sender")
                                .Include("EventLog.Comments")
                              where dbKeys.Contains(evt.EventLogId)
                              select evt).ToList().Cast<IOsbideEvent>().ToList();
                }
                foreach (IOsbideEvent evt in events)
                {
                    FeedItem item = new FeedItem();
                    item.Event = evt;
                    item.EventId = evt.Id;
                    item.Log = evt.EventLog;
                    item.LogId = evt.EventLogId;
                    item.Comments = evt.EventLog.Comments.ToList();
                    feedItems.Add(item.Log.DateReceived, item);
                }
            }

            //pull comments for all feed items
            Dictionary<int, FeedItem> itemsDict = new Dictionary<int, FeedItem>();
            foreach (FeedItem item in feedItems.Values)
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

            return feedItems.Values.Reverse().ToList();
        }
    }
}