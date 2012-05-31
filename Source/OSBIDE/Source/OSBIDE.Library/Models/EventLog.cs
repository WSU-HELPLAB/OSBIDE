using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;
using System.Runtime.Serialization;

namespace OSBIDE.Library.Models
{
    [Serializable]
    [DataContract]
    public class EventLog
    {
        [Key]
        [Required]
        [DataMember]
        public int Id { get; set; }

        [Required]
        [DataMember]
        public string LogType { get; set; }

        [Required]
        [DataMember]
        public DateTime DateReceived { get; set; }

        [Required]
        [DataMember]
        [Column(TypeName = "image")]
        public byte[] Data { get; set; }

        [Required]
        [DataMember]
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        [IgnoreDataMember]
        public virtual OsbideUser Sender { get; set; }

        public EventLog()
        {
            DateReceived = DateTime.Now;
        }

        public EventLog(IOsbideEvent evt, OsbideUser sender) 
        {
            DateReceived = DateTime.Now;
            LogType = evt.EventName;
            Data = EventFactory.ToZippedBinary(evt);

            //were we sent a null user?
            if (sender.FirstName == null && sender.LastName == null)
            {
                //replace with a generic user
                sender = OsbideUser.GenericUser();
            }
            Sender = sender;
            if (sender.Id != 0)
            {
                SenderId = sender.Id;
            }
        }

        public EventLog(EventLog copyLog)
        {
            DateReceived = copyLog.DateReceived;
            Id = copyLog.Id;
            LogType = copyLog.LogType;
            Data = copyLog.Data;
            Sender = new OsbideUser(copyLog.Sender);
            SenderId = copyLog.SenderId;
        }
    }
}
