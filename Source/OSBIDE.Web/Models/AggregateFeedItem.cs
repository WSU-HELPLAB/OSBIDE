using OSBIDE.Library.Events;
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
            /* AC: While this works, it won't serve at the moment
            LinkedList<FeedItem> feedItemsLinkedList = new LinkedList<FeedItem>(feedItems);
            LinkedListNode<FeedItem> node = feedItemsLinkedList.First;
            LinkedListNode<FeedItem> next;
            bool isOnBuildEvent = false;
            int lastAuthorId = 0;
            IOsbideEvent lastEvent = new BuildEvent();
            while (node != null)
            {
                next = node.Next;
                if (node.Value != null)
                {
                    if (node.Value.Log.LogType.CompareTo(BuildEvent.Name) == 0)
                    {
                        BuildEvent build = node.Value.Event as BuildEvent;
                        if (build != null)
                        {
                            //is this an empty error build event?
                            if (build.CriticalErrorCount == 0)
                            {
                                //have we already seen one immediately before this one?  If so,
                                //remove from the event chain.
                                if (isOnBuildEvent == true 
                                    && lastAuthorId == node.Value.Log.SenderId
                                    && lastEvent.SolutionName.CompareTo(node.Value.Event.SolutionName) == 0
                                    )
                                {
                                    feedItemsLinkedList.Remove(node);
                                }
                                else
                                {
                                    isOnBuildEvent = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        isOnBuildEvent = false;
                    }
                }
                lastAuthorId = node.Value.Log.SenderId;
                lastEvent = node.Value.Event;
                node = next;
            }
            feedItems = feedItemsLinkedList.ToList();
             * */
            List<AggregateFeedItem> aggregateItems = new List<AggregateFeedItem>();

            //prime the loop
            FeedItem previousItem = null;
            AggregateFeedItem currentAggregate = null;
            if (feedItems.Count > 0)
            {
                currentAggregate = new AggregateFeedItem(feedItems[0]);
                aggregateItems.Add(currentAggregate);
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