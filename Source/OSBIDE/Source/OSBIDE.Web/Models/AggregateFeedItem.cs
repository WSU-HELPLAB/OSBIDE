using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models
{
    public class AggregateFeedItem
    {
        public List<FeedItem> Items { get; set; }
        public string FeedItemType { get; set; }
        public List<LogComment> Comments { get; set; }
        public OsbideUser Creator { get; set; }
        public DateTime MostRecentOccurance { get; set; }
        public string PrettyName { get; set; }
        public int HelpfulMarks { get; set; }

        public static string[] AcceptableAggregates
        {
            get
            {
                string[] aggregates = {
                                          "DebugEvent",
                                          "ExceptionEvent"
                                      };
                return aggregates;
            }
        }

        public AggregateFeedItem()
        {
            Items = new List<FeedItem>();
            Comments = new List<LogComment>();
        }

        public AggregateFeedItem(FeedItem item)
            : this()
        {
            Items.Add(item);
            PrettyName = item.Event.PrettyName;
            FeedItemType = item.Event.EventName;
            Comments = Comments.Union(item.Comments).ToList();
            Creator = item.Log.Sender;
            MostRecentOccurance = item.Event.EventDate;
            HelpfulMarks = item.HelpfulComments;
        }

        public static List<AggregateFeedItem> FromFeedItems(IList<FeedItem> feedItems)
        {
            List<AggregateFeedItem> aggregateItems = new List<AggregateFeedItem>();

            //prime the loop
            FeedItem previousItem = null;
            AggregateFeedItem currentAggregate = null;
            if (feedItems.Count > 0)
            {
                currentAggregate = new AggregateFeedItem(feedItems[0]);
                aggregateItems.Add(currentAggregate);
                currentAggregate.HelpfulMarks += feedItems[0].HelpfulComments;
                previousItem = feedItems[0];
            }
            for (int i = 1; i < feedItems.Count; i++)
            {
                FeedItem item = feedItems[i];

                if (
                    AcceptableAggregates.Contains(item.Event.EventName) == true             //Do we care about this type of event?
                    && previousItem.Log.SenderId == item.Log.SenderId                       //Is the sender the same?
                    && previousItem.Event.EventName.CompareTo(item.Event.EventName) == 0    //is it of the same event type?
                    )
                {
                    currentAggregate.Items.Add(item);
                    currentAggregate.Comments = currentAggregate.Comments.Union(item.Comments).ToList();
                    currentAggregate.HelpfulMarks += item.HelpfulComments;
                }
                else
                {
                    currentAggregate = new AggregateFeedItem(item);
                    aggregateItems.Add(currentAggregate);
                }
                previousItem = item;
            }
            return aggregateItems;
        }
    }
}