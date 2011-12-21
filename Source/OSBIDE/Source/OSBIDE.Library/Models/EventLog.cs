using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;

namespace OSBIDE.Library.Models
{
    public class EventLog
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string LogType { get; set; }

        [Required]
        public DateTime DateReceived { get; set; }

        [Required]
        public byte[] Data { get; set; }

        [Required]
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public virtual OsbideUser Sender { get; set; }

        /// <summary>
        /// Whether or not the log item has been handled (sent to the server)
        /// </summary>
        [Required]
        public bool Handled { get; set; }

        public EventLog()
        {
            Handled = false;
            DateReceived = DateTime.Now;
        }

        public EventLog(IOsbideEvent evt, OsbideUser sender) 
        {
            Handled = false;
            DateReceived = DateTime.Now;
            LogType = evt.EventName;
            Data = EventFactory.ToZippedBinary(evt);
            Sender = sender;
            if (sender.Id != 0)
            {
                SenderId = sender.Id;
            }
        }
    }
}
