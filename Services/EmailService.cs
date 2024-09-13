using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Synchronous method for sending email
    public void SendEmail(string recipientEmail, string subject, string body)
    {
        // Declare variables first, then use them
        string smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException(nameof(smtpServer));
        int smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? throw new ArgumentNullException(nameof(smtpPort)));
        string senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException(nameof(senderEmail));
        string senderName = _configuration["EmailSettings:SenderName"] ?? throw new ArgumentNullException(nameof(senderName));
        string username = _configuration["EmailSettings:Username"] ?? throw new ArgumentNullException(nameof(username));
        string password = _configuration["EmailSettings:Password"] ?? throw new ArgumentNullException(nameof(password));

        var smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true // true if sending HTML email
        };

        mailMessage.To.Add(recipientEmail);

        smtpClient.Send(mailMessage); // Synchronous send
    }

    // Asynchronous method for sending email
    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        // Declare variables first, then use them
        string smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException(nameof(smtpServer));
        int smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? throw new ArgumentNullException(nameof(smtpPort)));
        string senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException(nameof(senderEmail));
        string senderName = _configuration["EmailSettings:SenderName"] ?? throw new ArgumentNullException(nameof(senderName));
        string username = _configuration["EmailSettings:Username"] ?? throw new ArgumentNullException(nameof(username));
        string password = _configuration["EmailSettings:Password"] ?? throw new ArgumentNullException(nameof(password));

        var smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true // true if sending HTML email
        };

        mailMessage.To.Add(recipientEmail);

        await smtpClient.SendMailAsync(mailMessage); // Asynchronous send
    }
}
