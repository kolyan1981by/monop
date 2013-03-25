using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;


namespace Monop.Data.Helpers
{
    public class EmailHelper
    {
        public static void SendEmail(string to, string subject, string content)
        {
            MailMessage message = new MailMessage();
            message.To.Add(new MailAddress(to));

            message.Subject = subject;
            message.Body = content;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Send(message);
        }

        public static void SendAsyncMail(string to, string subject, string content)
        {
            MailMessage mail = new MailMessage();

            mail.To.Add(new MailAddress(to));

            mail.Subject = subject;
            mail.Body = content;
            mail.IsBodyHtml = true;

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.EnableSsl = true;
            Object state = mail;

            //event handler for asynchronous call
            smtpClient.SendCompleted += new SendCompletedEventHandler(smtpClient_SendCompleted);
            try
            {
                smtpClient.SendAsync(mail, state);
            }
            catch (Exception ex)
            {

            }
        }
        static void smtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            MailMessage mail = e.UserState as MailMessage;

            if (!e.Cancelled && e.Error != null)
            {
               
            }
        }
    }
}
