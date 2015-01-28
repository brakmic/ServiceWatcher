using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Web;

namespace ServiceWatcherWebRole.Helpers
{
    /// <summary>
    /// Helper class for configuration access & email services
    /// </summary>
    public static class AdvaricsHelper
    {
        private static string smtpServer;
        private static string smtpUser;
        private static string smtpPassword;
        private static bool smtpEnableSsl;
        private static int smtpPort;
        private static string smtpSender;
        private static List<string> smtpRecipients;

        static AdvaricsHelper()
        {
            LoadSmtpSettings();
            LoadRxSettings();
        }

        private static void LoadRxSettings()
        {
            // TODO
        }
        /// <summary>
        /// Load SMT settings
        /// </summary>
        public static void LoadSmtpSettings()
        {
            smtpServer = GetConfigurationSettingValue("SmtpServer");
            smtpUser = GetConfigurationSettingValue("SmtpUser");
            smtpPassword = GetConfigurationSettingValue("SmtpPassword");
            smtpSender = GetConfigurationSettingValue("SmtpSender");
            smtpEnableSsl = bool.Parse(GetConfigurationSettingValue("SmtpEnableSsl"));
            smtpPort = int.Parse(GetConfigurationSettingValue("SmtpPort"));
            smtpRecipients = new List<string>(GetConfigurationSettingValue("SmtpRecipients").
                                                                            Split(new[] { ',' },
                                                                            StringSplitOptions.RemoveEmptyEntries));
        }
        /// <summary>
        /// Helper method for sending emails
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public static void SendEmail(string subject, string message)
        {
            if (smtpRecipients != null &&
                smtpRecipients.Count > 0)
            {
                SmtpClient smtpClient;
                MailMessage eMail;

                smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.EnableSsl = smtpEnableSsl;
                smtpClient.Credentials = new NetworkCredential(smtpSender, smtpPassword);

                foreach (var recipient in smtpRecipients)
                {
                    eMail = new MailMessage(new MailAddress(smtpSender), new MailAddress(recipient));
                    eMail.Body = message;
                    eMail.Subject = subject;
                    try
                    {
                        smtpClient.SendAsync(eMail, recipient);
                        smtpClient.SendCompleted += smtpClient_SendCompleted;
                    }
                    catch (Exception ex)
                    {
                        string exMsg = ConsumeException(MethodInfo.GetCurrentMethod().Name, ex.Message, ex);
                        string errorInfo = string.Format("Could not send email.\r\nSubject: {0},\r\nmessage: {1},\r\nError: {2}", subject, message, exMsg);
                        AdvaricsHelper.Log(errorInfo, "Warning");
                    }
                }
            }
            else
            {
                AdvaricsHelper.Log("Could not sent an email because there are no recipients.", "Warning");
            }
        }

        static void smtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string recipient = e.UserState as string;
            if (e.Error != null)
            {
                AdvaricsHelper.Log(string.Format("Sending E-Mail to {0} failed. Error {1}",
                                        recipient,
                                        AdvaricsHelper.ConsumeException(MethodInfo.GetCurrentMethod().Name,
                                                             e.Error.Message, e.Error)), "Error");
            }
            else if (e.Cancelled)
            {
                AdvaricsHelper.Log(string.Format("Sending E-Mail to {0} cancelled.", recipient), "Warning");
            }
            else
            {
                AdvaricsHelper.Log(string.Format("E-Mail successfully sent to {0}", recipient));
            }
        }

        /// <summary>
        /// Get config settings from Azure or localhost
        /// </summary>
        /// <param name="configSettingName"></param>
        /// <returns></returns>
        public static string GetConfigurationSettingValue(string configSettingName)
        {
            try
            {
                if (RoleEnvironment.IsAvailable)
                {
                    return RoleEnvironment.GetConfigurationSettingValue(configSettingName);
                }
                else
                {
                    return ConfigurationManager.AppSettings[configSettingName];
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not read setttings from config.", ex.InnerException);
            }
        }
        /// <summary>
        /// Get connection string
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <returns></returns>
        public static string GetConnectionStringValue(string connectionStringName)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get connection string from config.", ex.InnerException);
            }
        }

        /// <summary> 
        /// Gets complete exception info from an exception instance 
        /// </summary> 
        /// <param dataSourceName="where">Where the exception happended</param> 
        /// <param dataSourceName="what">Short description</param> 
        /// <param dataSourceName="ex">Exception instance for traversing the cascaded information</param> 
        public static string ConsumeException(string where = "", string what = "", Exception ex = null)
        {
            string info = string.Empty;
            if (ex != null)
            {
                var ex2 = new Exception(where, new Exception(what, ex));
                info = ExtractErrorInfo(ref ex2);
            }
            return info;
        }
        /// <summary> 
        /// get all exception information (InnerException instances included) 
        /// </summary> 
        /// <param dataSourceName="ex">Exception instance</param> 
        /// <returns>Assembled exception information</returns> 
        private static string ExtractErrorInfo(ref Exception ex)
        {
            string info = null;
            while (ex != null)
            {
                info += "\r\n" + ex; // ex.Message; 
                ex = ex.InnerException;
            }
            return info;
        }
        /// <summary>
        /// A simple logging method
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Log(string message, string type = "Information")
        {
            string formatted = string.Format("[{0}] {1}", DateTime.UtcNow, message);
            if (type.Equals("Warning"))
            {
                Trace.TraceWarning(formatted);
            }
            else if (type.Equals("Error"))
            {
                Trace.TraceError(formatted);
            }
            else
            {
                Trace.TraceInformation(formatted);
            }
        }
    }
}