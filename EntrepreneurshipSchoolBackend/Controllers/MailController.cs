using System.Collections.Concurrent;
using EntrepreneurshipSchoolBackend.DTOs;
using EntrepreneurshipSchoolBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using Task = System.Threading.Tasks.Task;

namespace EntrepreneurshipSchoolBackend.Controllers;

/// <summary>
/// This controller is responsible for operating endpoints of transactions.
/// </summary>
[ApiController]
public class MailController : ControllerBase
{
    private readonly ApiDbContext _context;

    /// <summary>
    /// Create the controller with a dependency injection of a database context.
    /// </summary>
    /// <param name="context">Context required to work with a database.</param>
    public MailController(ApiDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Create e-mail
    /// </summary>
    /// <param name="mailInfo">E-mail to create</param>
    /// <returns>200</returns>
    [HttpPost("/admin/mail")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateMail(MailDTO mailInfo)
    {
        var emails = new ConcurrentDictionary<string, MailboxAddress>();
        foreach (var grouping in mailInfo.grouping)
        {
            switch (grouping)
            {
                case "All":
                    await _context.Learner.ForEachAsync(x => emails.TryAdd(x.EmailLogin,
                        new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin)));
                    break;
                case "Trackers":
                    await _context.Learner.Where(x => x.IsTracker == '1').ForEachAsync(x =>
                        emails.TryAdd(x.EmailLogin,
                            new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin)));
                    break;
                case "Learners":
                    await _context.Learner.Where(x => x.IsTracker == '0').ForEachAsync(x =>
                        emails.TryAdd(x.EmailLogin,
                            new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin)));
                    break;
            }
        }

        await _context.Learner.Where(x => mailInfo.learnersIds.Contains(x.Id)).ForEachAsync(x =>
            emails.TryAdd(x.EmailLogin, new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin)));
        await _context.Relates.Include(x => x.Learner)
            .Where(x => mailInfo.teamsIds.Contains(x.GroupId)).Select(x => x.Learner)
            .ForEachAsync(x =>
                emails.TryAdd(x.EmailLogin, new MailboxAddress($"{x.Surname} {x.Name} {x.Lastname}", x.EmailLogin)));

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(Properties.EmailName, Properties.EmailAddress));
        message.To.AddRange(emails.Values);
        message.Subject = mailInfo.title;
        
        message.Body = new TextPart("plain") { Text = mailInfo.content };
        
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(Properties.SmtpAddress, Properties.SmtpPort, Properties.SmtpUseSsl);
        
            await client.AuthenticateAsync(Properties.EmailAddress, Properties.EmailPassword);
        
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        return new OkResult();
    }
}