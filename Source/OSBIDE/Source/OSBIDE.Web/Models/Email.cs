using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace OSBIDE.Web.Models
{
    public class Email
    {
        public static void Send(string subject, string message, ICollection<MailAddress> to)
        {
#if !DEBUG
            //ignore empty sends
            if (to.Count == 0)
            {
                return;
            }

            SmtpClient mailClient = new SmtpClient();

            MailAddress fromAddress = new MailAddress(ConfigurationManager.AppSettings["OsbideFromEmail"], "OSBIDE");

            foreach (MailAddress recipient in to)
            {
                MailMessage mm = new MailMessage();

                mm.From = fromAddress;
                mm.To.Add(recipient);
                mm.Subject = subject;
                mm.Body = message;
                mm.IsBodyHtml = true;

                //bomb's away!
                mailClient.Send(mm);
            }
            
            mailClient.Dispose();
#endif
        }
    }
}