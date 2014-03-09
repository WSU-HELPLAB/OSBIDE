﻿using OSBIDE.Data.DomainObjects;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Queries;
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
        public List<LogCommentEvent> Comments { get; set; }
        public OsbideUser Creator { get; set; }
        public DateTime MostRecentOccurance { get; set; }
        public string PrettyName { get; set; }
        public int HelpfulMarks { get; set; }
        public bool IsAnonymous { get; private set; }

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
            Comments = new List<LogCommentEvent>();
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

            //IDE events are anonymized
            var ideEvents = ActivityFeedQuery.GetIdeEvents();
            bool isIde = (from evt in ideEvents
                          where evt.EventName == item.Event.EventName
                          select evt).FirstOrDefault() != null;
            if (isIde)
            {
                IsAnonymous = true;
            }
        }

        public static List<AggregateFeedItem> FromFeedItems(IList<FeedItem> feedItems)
        {
            #region comments
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
            #endregion
            List<AggregateFeedItem> aggregateItems = new List<AggregateFeedItem>();

            //prime the loop
            FeedItem previousItem = null;
            AggregateFeedItem currentAggregate = null;
            if (feedItems.Count > 0)
            {
                currentAggregate = new AggregateFeedItem(feedItems[0]);

                //AC: LogCommentEvents work a little differenty.  Their comments come from the event to which they point
                //    and not themselves.
                if (feedItems[0].Event.EventName == LogCommentEvent.Name)
                {
                    currentAggregate.Comments = currentAggregate.Comments.Union((feedItems[0].Event as LogCommentEvent).SourceEventLog.Comments).ToList();
                }

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
                    
                    //AC: LogCommentEvents work a little differenty.  Their comments come from the event to which they point
                    //    and not themselves.
                    if (item.Event.EventName == LogCommentEvent.Name)
                    {
                        currentAggregate.Comments = currentAggregate.Comments.Union((item.Event as LogCommentEvent).SourceEventLog.Comments).ToList();
                    }
                    aggregateItems.Add(currentAggregate);
                }
                previousItem = item;
            }


            return aggregateItems;
        }
    }
}