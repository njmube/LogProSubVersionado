using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities.Report;
using Entities.Profile;
using System.Net.Mail;
using System.Net;
using Integrator.Dao;
using Integrator;
using Entities;

namespace UtilTool
{
    public class MailSender
    {      
        private static String Server { get; set; }
        private static String User { get; set; }
        private static String Password { get; set; }
        private static String Domain { get; set; }
        private static String HtmlEnvelope { get; set; }
        private static bool EnableSSL { get; set; }

        private static void SetMailOptions() {

            DaoFactory factory = null;

            if (string.IsNullOrEmpty(Server)) {

                factory = new DaoFactory();

                Server = factory.DaoConfigOption().Select(
                    new ConfigOption { Code = "MAILSERVER" }).First().DefValue;

                User = factory.DaoConfigOption().Select(
                    new ConfigOption { Code = "MAILUSER" }).First().DefValue;

                Password = factory.DaoConfigOption().Select(
                    new ConfigOption { Code = "MAILPASSWD" }).First().DefValue;

                Domain = factory.DaoConfigOption().Select(
                    new ConfigOption { Code = "MAILDOMAIN" }).First().DefValue;

                HtmlEnvelope = factory.DaoConfigOption().Select(
                    new ConfigOption { Code = "HTMLENVELOPE" }).First().DefValue;

                try
                {
                    EnableSSL = factory.DaoConfigOption().Select(
                      new ConfigOption { Code = "MAILSSL" }).First().DefValue == "T";
                }
                catch { EnableSSL = false; }

            }

        }


        public static bool SendEmail(MessagePool message, string[] attachments) {

            try
            {

                if (string.IsNullOrEmpty(message.MailTo))
                    return true;

                SetMailOptions();

                message.Message =  HtmlEnvelope.Replace("__CONTENT", message.Message);

                MailMessage objMessage = new MailMessage();

                objMessage.From = new MailAddress(message.MailFrom);

                try
                {
                    foreach (String address in message.MailTo.Split(';'))
                        objMessage.To.Add(address.Trim());
                }
                catch { return true; }
                
                objMessage.Subject = message.Subject;
                objMessage.Body = message.Message;

                // CAA [2010/07/27] adjuntos del correo}
                try
                {
                    foreach (string adj in attachments)
                    {
                        Attachment att = new Attachment(adj);
                        objMessage.Attachments.Add(att);
                    }
                }
                catch { }


                objMessage.IsBodyHtml = true;

                //objMessage.Priority = MailPriority.High;

                SmtpClient smtpClient = new SmtpClient(Server);

                if (string.IsNullOrEmpty(User))
                    smtpClient.UseDefaultCredentials = true;

                else
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(User, Password, Domain);
                }


                if (EnableSSL)
                    smtpClient.EnableSsl = true;

                smtpClient.Send(objMessage);
                return true;
            }
            catch (Exception ex)
            {
                ExceptionMngr.WriteEvent("SendEmail: ", ListValues.EventType.Fatal, ex, null, ListValues.ErrorCategory.Messages);
                return false;
            }


        }

    }
}
