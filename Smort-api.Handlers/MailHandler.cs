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
public class MailHandler :IDisposable
{
    protected string MailAccount { get; set; }
    protected string Password { get; set; }
    protected string SMTPserver { get; set; }
    protected string port { get; set; }
    protected SmtpClient smtp;
    public MailHandler()
    {

        MailAccount = Environment.GetEnvironmentVariable("SmtpMail") ?? "";
        Password = Environment.GetEnvironmentVariable("SmtpPassword") ?? "";
        SMTPserver = Environment.GetEnvironmentVariable("SmtpServer") ?? "";
        port = Environment.GetEnvironmentVariable("SmtpPort") ?? "";

        int portInt = 0;
        if (int.TryParse(port, out portInt))
        {
            smtp = new SmtpClient();
            smtp.Host = SMTPserver;
            smtp.Port = portInt;
            smtp.Credentials = new NetworkCredential(MailAccount, Password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;
        }
    }

    public void Dispose()
    {
        if(smtp != null)
        {
            smtp.Dispose();
        }
    }

    public void SendMail(string UserEmail)
    {
        try
        {
            Console.WriteLine(UserEmail);
            MailAddress to = new MailAddress(UserEmail);
            MailAddress from = new MailAddress(MailAccount);

            MailMessage email = new MailMessage(from, to);
            email.Subject = "Welcome To Smorthub";
            email.Body = "Hello, Your account is not yet active please wait for the admin to aprove your account.";

            smtp.Send(email);
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine("Could not send an mail to user...");
        }
    }
}
