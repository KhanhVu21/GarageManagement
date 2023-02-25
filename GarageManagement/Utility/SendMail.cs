using MimeKit;
using MailKit.Net.Smtp;


namespace GarageManagement.Utility
{
    public class SendMail
    {
        #region FUNCTION
        //FUNCTION SEND MAIL AUTOMATION COMMON
        public void SendMailAuto(string fromMail, string toMail, string password, string Subject, string Body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Garage 247", fromMail));
            message.To.Add(new MailboxAddress("Recipient Name", toMail));
            message.Subject = Subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = Body;

            message.Body = bodyBuilder.ToMessageBody();

            // send the email
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate(fromMail, password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
        #endregion
    }
}
