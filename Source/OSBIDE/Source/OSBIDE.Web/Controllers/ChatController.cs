using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [RequiresVisualStudioConnectionForStudents]
    public class ChatController : ControllerBase
    {
        public ActionResult Index(int id = -1, int sendingRoomId = -1)
        {
            if (id == -1)
            {
                //find the default chat room id
                id = Db.ChatRooms.Where(r => r.SchoolId == CurrentUser.SchoolId)
                    .Where(r => r.IsDefaultRoom == true)
                    .FirstOrDefault()
                    .Id;
            }
            if (sendingRoomId != -1)
            {
                string region = string.Format("chat_{0}", sendingRoomId);
                GlobalCache.Remove(CurrentUser.Id.ToString(), region);
            }
            DateTime minDate = DateTime.Now.Subtract(new TimeSpan(0, 1, 0, 0));

            //get all chat messages that are associated with the requested room and have been issued within the last hour
            List<ChatMessage> chatMessages = (from message in Db.ChatMessages
                                  .Include("Author")
                                              where message.RoomId == id
                                              && message.MessageDate > minDate
                                              select message).ToList();
            ChatRoom room = Db.ChatRooms.Where(r => r.Id == id).FirstOrDefault();
            ChatRoomViewModel vm = new ChatRoomViewModel()
            {
                Messages = chatMessages,
                ActiveRoom = room,
                InitialDocumentDate = minDate,
                Rooms = Db.ChatRooms.Where(r => r.SchoolId == CurrentUser.SchoolId).ToList(),
                Users = Db.Users.Where(u => u.SchoolId == CurrentUser.SchoolId).OrderBy(u => u.FirstName).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public ActionResult OldMessages(int chatRoomId, long firstMessageTick, int count)
        {
            DateTime firstMessage = new DateTime(firstMessageTick);
            List<ChatMessage> messages = new List<ChatMessage>();
            messages = (from message in Db.ChatMessages
                                  .Include("Author")
                              where message.RoomId == chatRoomId
                              && message.MessageDate < firstMessage
                              select message).ToList();
            ChatRoom room = Db.ChatRooms.Where(r => r.Id == chatRoomId).FirstOrDefault();
            ChatRoomViewModel vm = new ChatRoomViewModel()
            {
                Messages = messages,
                ActiveRoom = room
            };
            return View("RecentMessages", vm);
        }

        [HttpGet]
        public ActionResult RecentMessages(int chatRoomId, long lastMessageTick)
        {
            int timeout = 40;
            int timeoutCounter = 0;
            TimeSpan timeBetweenQueries = new TimeSpan(0, 0, 0, 1);
            bool hasRequestTimedOut = false;
            DateTime lastMessage = new DateTime(lastMessageTick);
            List<ChatMessage> recentMessages = new List<ChatMessage>();

            while (recentMessages.Count == 0 && hasRequestTimedOut == false)
            {
                recentMessages = (from message in Db.ChatMessages
                                  .Include("Author")
                                  where message.RoomId == chatRoomId
                                  && message.MessageDate > lastMessage
                                  select message).ToList();
                if (recentMessages.Count == 0)
                {
                    Thread.Sleep(timeBetweenQueries);
                    timeoutCounter++;
                    if (timeoutCounter == timeout)
                    {
                        hasRequestTimedOut = true;
                    }
                }
            }
            ChatRoom room = Db.ChatRooms.Where(r => r.Id == chatRoomId).FirstOrDefault();
            ChatRoomViewModel vm = new ChatRoomViewModel()
            {
                Messages = recentMessages,
                ActiveRoom = room
            };
            return View(vm);
        }

        public JsonResult RoomUsers(int chatRoomId)
        {
            //record the user as being logged into the room
            string region = string.Format("chat_{0}", chatRoomId);
            GlobalCache.Add(CurrentUser.Id.ToString(), CurrentUser.FirstAndLastName, new CacheItemPolicy() { SlidingExpiration = new TimeSpan(0, 0, 0, 10) }, region);

            List<ChatRoom> rooms = Db.ChatRooms.ToList();
            List<ChatRoomUsersViewModel> vms = new List<ChatRoomUsersViewModel>();
            foreach (ChatRoom room in rooms)
            {
                region = string.Format("chat_{0}", room.Id);
                Dictionary<int, string> users = new Dictionary<int, string>();
                string[] keys = null;
                try
                {
                    keys = GlobalCache.GetKeys(region);
                }
                catch (Exception)
                {
                    //cache doesn't exist.  continue onto next iteration of main loop.
                    continue;
                }
                foreach (string key in keys)
                {
                    if (string.IsNullOrEmpty(key) == false)
                    {
                        int userId = -1;
                        if (Int32.TryParse(key, out userId) == true)
                        {
                            vms.Add(new ChatRoomUsersViewModel()
                            {
                                RoomId = room.Id,
                                UserId = userId
                            });
                        }
                    }
                }
            }
            return this.Json(vms, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostMessage(int chatRoomId, string message)
        {
            var result = PostMessageAsync(chatRoomId, message);
            return RedirectToAction("Index", new { id = chatRoomId });
        }

        [HttpPost]
        public JsonResult PostMessageAsync(int chatRoomId, string message)
        {
            int messageId = -1;
            DateTime messageTimestamp = DateTime.MinValue;
            var result = new { id = -1, timestamp = -1 };
            if (Db.ChatRooms.Where(r => r.Id == chatRoomId).Count() > 0)
            {
                if (string.IsNullOrEmpty(message) == false)
                {
                    ChatMessage chat = new ChatMessage()
                    {
                        AuthorId = CurrentUser.Id,
                        Message = message,
                        MessageDate = DateTime.Now,
                        RoomId = chatRoomId
                    };
                    Db.ChatMessages.Add(chat);
                    Db.SaveChanges();
                    messageId = chat.Id;
                    messageTimestamp = chat.MessageDate;
                }
            }
            return this.Json(new { id = messageId, timestamp = messageTimestamp.Ticks });
        }
    }
}
