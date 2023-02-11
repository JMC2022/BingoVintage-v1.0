using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BingoVintage.ExtentionMethods
{
    public class EmailMethod    
    {
        public void SendEmail(string emailTo, string nameTo, string token)
        {
            //Mailkit resourses
            using var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
            "Bingo vintage",
            "noreply@bingovintage.com"
            ));
            email.To.Add(new MailboxAddress($"{nameTo}", $"{emailTo}"));
            email.Subject = $"Welcome to BingoVintage";
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"Your Verification Token is: {token}"
            };
            email.Body = bodyBuilder.ToMessageBody();

            //Send email
            using SmtpClient client = new();
            client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            client.Authenticate(
                userName: "bingovintage2022@gmail.com",
                password: "mwulkmjftcdwqwwe"
            );
            client.Send(email);
            client.Disconnect(true);
        }
    }
}
