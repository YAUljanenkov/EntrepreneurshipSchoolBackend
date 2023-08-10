using MailKit.Net.Smtp;
using MimeKit;

namespace EntrepreneurshipSchoolBackend.Utility;

/// <summary>
/// Class-wrapper for sending e-mails. Depends on properities.
/// </summary>
public static class Mail
{
    /// <summary>
    /// This methods sends e-mails.
    /// </summary>
    /// <param name="emails">A list of mail addresses to send to.</param>
    /// <param name="title">A title of an e-mail.</param>
    /// <param name="content">Text of a message.</param>
    public static async Task SendMessages(IEnumerable<MailboxAddress> emails, string title, string content)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(Properties.SmtpAddress, Properties.SmtpPort, Properties.SmtpUseSsl);
        await client.AuthenticateAsync(Properties.EmailAddress, Properties.EmailPassword);
        foreach (var mailboxAddress in emails)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(Properties.EmailName, Properties.EmailAddress));
            message.To.Add(mailboxAddress);
            message.Subject = title;
            message.Body = new TextPart("plain") { Text = content };
            try
            {
                await client.SendAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        await client.DisconnectAsync(true);
    }

}