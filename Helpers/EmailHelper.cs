using System;
using System.Net;
using System.Net.Mail;

public static class EmailHelper
{

    private static readonly string fromEmail = "phamvantung.cr25@gmail.com";         
    private static readonly string smtpUser = "phamvantung.cr25@gmail.com";         
    private static readonly string smtpPass = "culeckhjdpdxoknv"; // <-- App Password 
    private static readonly string smtpHost = "smtp.gmail.com";
    private static readonly int smtpPort = 587;

    public static void GuiMail(string toEmail, string subject, string bodyHtml)
    {
        if (string.IsNullOrWhiteSpace(smtpPass))
            throw new InvalidOperationException("SMTP password not set. Thay smtpPass trong code bằng app-password.");

        var mail = new MailMessage();
        mail.From = new MailAddress(fromEmail, "Website của bạn");
        mail.To.Add(toEmail);
        mail.Subject = subject;
        mail.Body = bodyHtml;
        mail.IsBodyHtml = true;

        using (var smtp = new SmtpClient(smtpHost, smtpPort))
        {
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
            smtp.Send(mail);
        }
    }
}
