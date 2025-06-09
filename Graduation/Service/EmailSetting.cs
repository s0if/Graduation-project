using Graduation.DTOs.Email;
using System.Net.Mail;
using System.Net;

namespace Graduation.Service
{
    public class EmailSetting
    {
        public static void SendEmail(EmailDTOs Email)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("aqarandservices@gmail.com", "ivqn jzmf roln cbdh");
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("aqarandservices@gmail.com");
            mailMessage.To.Add(Email.Recivers);
            mailMessage.Subject = Email.Subject;
            mailMessage.Body = Email.Body;
            mailMessage.IsBodyHtml = true; 
            client.Send(mailMessage);
        }
    }
}
