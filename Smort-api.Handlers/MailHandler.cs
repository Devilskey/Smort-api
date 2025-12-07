using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;

namespace Smort_api.Handlers;
public class MailHandler 
{
    protected string MailAccount { get; set; }
    protected string Password { get; set; }
    protected string SMTPserver { get; set; }
    protected string port { get; set; }
    protected SmtpClient smtp;
    public MailHandler()
    {

        MailAccount = Environment.GetEnvironmentVariable("SmtpMail") ?? "smort@example.com";
        Password = Environment.GetEnvironmentVariable("SmtpPassword") ?? "";
        SMTPserver = Environment.GetEnvironmentVariable("SmtpServer") ?? "127.0.0.1";
        port = Environment.GetEnvironmentVariable("SmtpPort") ?? "25";

        int portInt = 0;
        if (int.TryParse(port, out portInt))
        {
            smtp = new SmtpClient();
            smtp.Host = SMTPserver;
            smtp.Port = portInt;
            smtp.Credentials = new NetworkCredential(MailAccount, Password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = false;
        }
    }

    public void Dispose()
    {
        if(smtp != null)
        {
            smtp.Dispose();
        }
    }

    public void SendMail(string userEmail, string body, string subject)
    {
        try
        {
            Console.WriteLine(userEmail);
            MailAddress to = new MailAddress(userEmail);
            MailAddress from = new MailAddress(MailAccount);

            MailMessage email = new MailMessage(from, to);
            email.Subject = subject;  
            email.Body = body;

            smtp.Send(email);
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine("Could not send an mail to user...");
        }
    }
}
