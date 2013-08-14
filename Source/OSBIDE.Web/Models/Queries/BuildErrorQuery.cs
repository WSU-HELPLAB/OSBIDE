using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.Queries
{
    public class BuildErrorQuery : ActivityFeedQuery, IOsbideQuery<FeedItem>
    {

        public int BuildErrorTypeId { get; set; }

        public BuildErrorQuery(OsbideContext db)
            : base(db)
        {
        }

        public override IList<FeedItem> Execute()
        {
            if (BuildErrorTypeId > 0)
            {
                var query = from error in _db.BuildErrors
                            join log in _db.EventLogs on error.LogId equals log.Id
                            join be in _db.BuildEvents on log.Id equals be.EventLogId
                            join comm in _db.LogCommentEvents on log.Id equals comm.SourceEventLogId into logComments
                            where log.DateReceived >= StartDate
                              && log.DateReceived <= EndDate
                              && log.Id > MinLogId
                              && error.BuildErrorTypeId == BuildErrorTypeId
                              orderby log.DateReceived descending
                            select new
                            {
                                Log = log,
                                BuildEvent = be,
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
                List<FeedItem> feedItems = new List<FeedItem>();
                foreach (var row in query)
                {
                    FeedItem item = new FeedItem();
                    item.LogId = row.Log.Id;
                    item.Log = row.Log;
                    item.Comments = row.Comments.ToList();
                    item.HelpfulComments = row.HelpfulMarks;
                    item.EventId = row.BuildEvent.Id;
                    item.Event = row.BuildEvent;
                    feedItems.Add(item);
                }
                return feedItems;

            }
            return new List<FeedItem>();
        }
    }
}