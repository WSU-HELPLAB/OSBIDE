using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class GetHashtagsProc
    {
        public static List<Tag> Run(string query, bool isHandle)
        {
            using (var context = new OsbideProcs())
            {
                return context.GetHashtags(query, isHandle).Select(t => new Tag { Id = t.Id, Name = t.Tag, IsHandle = t.IsHandle }).ToList();
            }
        }

        public static List<Tag> GetTrends(int topN)
        {
            using (var context = new OsbideProcs())
            {
                return context.GetTrends(topN).Select(t => new Tag { Id = t.HashtagId, Name = t.Hashtag, IsHandle = false }).ToList();
            }
        }

        public static List<Notification> GetNotifications(int userId, int topN, bool getAll)
        {
            using (var context = new OsbideProcs())
            {
                return context.GetNotifications(userId, topN, getAll)
                                .Select(t => new Notification
                                {
                                    FirstName = t.FirstName,
                                    LastName = t.LastName,
                                    Viewed = t.Viewed,
                                    UserId = t.UserId,
                                    EventLogId = t.EventLogId
                                })
                                .ToList();
            }
        }
    }
}
