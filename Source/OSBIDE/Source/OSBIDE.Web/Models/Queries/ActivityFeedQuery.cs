using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
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
            //TODO: Are these the only three that we care to show in the feed?
            List<IOsbideEvent> events = new List<IOsbideEvent>();
            events.Add(new BuildEvent());
            events.Add(new ExceptionEvent());
            events.Add(new FeedCommentEvent());
            events.Add(new AskForHelpEvent());
            events.Add(new SubmitEvent());
            return events;
        }

        public void AddEventType(IOsbideEvent evt)
        {
            eventSelectors.Add(evt);
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

            //The big one.  Basically, I'm crafting one huge join to all possible event types that we log.
            var query = from log in _db.EventLogs.Include("Sender.Score")
                        join ae in _db.AskForHelpEvents on log.Id equals ae.EventLogId into askForHelpEvents
                        join be in _db.BuildEvents on log.Id equals be.EventLogId into buildEvents
                        join cce in _db.CutCopyPasteEvents on log.Id equals cce.EventLogId into cutCopyPasteEvents
                        join dbe in _db.DebugEvents on log.Id equals dbe.EventLogId into debugEvents
                        join eae in _db.EditorActivityEvents on log.Id equals eae.EventLogId into editorActivityEvents
                        join ex in _db.ExceptionEvents on log.Id equals ex.EventLogId into exceptionEvents
                        join fi in _db.FeedCommentEvents on log.Id equals fi.EventLogId into feedCommentEvents
                        join sa in _db.SaveEvents on log.Id equals sa.EventLogId into saveEvents
                        join se in _db.SubmitEvents on log.Id equals se.EventLogId into submitEvents
                        join comm in _db.LogComments on log.Id equals comm.LogId into logComments
                        where log.DateReceived >= StartDate
                              && log.DateReceived <= EndDate
                              && log.Id > MinLogId
                        orderby log.DateReceived descending
                        select new
                        {
                            Log = log,
                            AskForHelpEvent = askForHelpEvents.FirstOrDefault(),
                            BuildEvent = buildEvents.FirstOrDefault(),
                            CutCopyPasteEvent = cutCopyPasteEvents.FirstOrDefault(),
                            DebugEvent = debugEvents.FirstOrDefault(),
                            EditorActivityEvent = editorActivityEvents.FirstOrDefault(),
                            ExceptionEvent = exceptionEvents.FirstOrDefault(),
                            FeedCommentEvent = feedCommentEvents.FirstOrDefault(),
                            SaveEvent = saveEvents.FirstOrDefault(),
                            SubmitEvent = submitEvents.FirstOrDefault(),
                            Comments = logComments,
                            HelpfulMarks = (from helpful in _db.HelpfulLogComments
                                            where logComments.Select(l => l.Id).Contains(helpful.CommentId)
                                            select helpful).Count()
                        };

            //were we supplied with a maximum ID number?
            if (MaxLogId > 0)
            {
                query = from q in query
                        where q.Log.Id < MaxLogId
                        select q;
            }

            //if we were asked to retrieve a certain list of events, add that into the query
            if (eventIds.Count > 0)
            {
                query = from q in query
                        where eventIds.Contains(q.Log.Id)
                        select q;
            }

            //filter by desired events if desired
            string[] eventNames = eventSelectors.Select(e => e.EventName).ToArray();
            if (eventNames.Length > 0)
            {
                query = from q in query
                        where eventNames.Contains(q.Log.LogType)
                        select q;
            }


            //get list of subscription ids
            int[] subjectIds = subscriptionSubjects.Select(s => s.Id).ToArray();
            if (subjectIds.Length > 0)
            {
                query = from q in query
                        where subjectIds.Contains(q.Log.SenderId)
                        select q;
            }

            //did we only want a certain number of results returned?
            if (MaxQuerySize > 0)
            {
                query = query.Take(MaxQuerySize);
            }

            //finally, loop through the query and build our results
            foreach (var row in query)
            {
                FeedItem item = new FeedItem();
                item.LogId = row.Log.Id;
                item.Log = row.Log;
                item.Comments = row.Comments.ToList();
                item.HelpfulComments = row.HelpfulMarks;

                //figure out what goes into the "Event" property
                IOsbideEvent nonNullLog = null;
                if (row.AskForHelpEvent != null)
                {
                    nonNullLog = row.AskForHelpEvent;
                }
                else if (row.BuildEvent != null)
                {
                    nonNullLog = row.BuildEvent;
                }
                else if (row.CutCopyPasteEvent != null)
                {
                    nonNullLog = row.CutCopyPasteEvent;
                }
                else if (row.DebugEvent != null)
                {
                    nonNullLog = row.DebugEvent;
                }
                else if (row.EditorActivityEvent != null)
                {
                    nonNullLog = row.EditorActivityEvent;
                }
                else if (row.ExceptionEvent != null)
                {
                    nonNullLog = row.ExceptionEvent;
                }
                else if (row.FeedCommentEvent != null)
                {
                    nonNullLog = row.FeedCommentEvent;
                }
                else if (row.SaveEvent != null)
                {
                    nonNullLog = row.SaveEvent;
                }
                else if (row.SubmitEvent != null)
                {
                    nonNullLog = row.SubmitEvent;
                }
                else
                {
                    //everything was null.  Skip this record.
                    continue;
                }

                //add in event information
                item.EventId = nonNullLog.Id;
                item.Event = nonNullLog;
                feedItems.Add(item);
            }

            return feedItems;
        }
    }
}