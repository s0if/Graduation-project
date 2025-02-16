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
            client.Credentials = new NetworkCredential("saifalkomi@gmail.com", "xfsw xyqq qxrw fped");
            client.Send("saifalkomi@gmail.com", Email.Recivers, Email.Subject, Email.Body);

        }
    }
}
