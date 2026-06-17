using System.Net;
using System.Net.Mail;
using BLL.Contracts;
using DTOs.FeedbackDtos;
using Microsoft.Extensions.Configuration;

namespace BLL.Services;

public class MailService : IMailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private const string AdminEmail = "valnos04@gmail.com";

    public MailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
        _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "";
        _fromName = _configuration["EmailSettings:FromName"] ?? "ShiftEase";
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        var resetLink = $"{_configuration["AppSettings:FrontendUrl"]}/reset-password?token={resetToken}";

        var subject = "Password Reset Request";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                <p>Or copy and paste this link into your browser:</p>
                <p>{resetLink}</p>
                <p>This link will expire in 1 hour.</p>
                <p>If you did not request a password reset, please ignore this email.</p>
                <br/>
                <p>Best regards,<br/>The ShiftEase Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendSupportNotificationAsync(string senderEmail, string subject, string message)
    {
        var emailSubject = $"[Support] {subject}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>New Support Message</h2>
                <table style='border-collapse: collapse; width: 100%;'>
                    <tr><td style='padding: 8px; font-weight: bold;'>From:</td><td style='padding: 8px;'>{senderEmail}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>Subject:</td><td style='padding: 8px;'>{subject}</td></tr>
                    <tr><td style='padding: 8px; font-weight: bold;'>Message:</td><td style='padding: 8px;'>{message}</td></tr>
                </table>
            </body>
            </html>
        ";

        await SendEmailAsync(AdminEmail, emailSubject, body);
    }

    public async Task SendFeedbackNotificationAsync(BllFeedbackResponse feedback)
    {
        var obstacle = string.IsNullOrWhiteSpace(feedback.Obstacle) ? "—" : feedback.Obstacle;
        var comments = string.IsNullOrWhiteSpace(feedback.AdditionalComments) ? "—" : feedback.AdditionalComments;

        var subject = $"[Feedback] Rating {feedback.Rating}/10";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>New Feedback Submission</h2>
                <table style='border-collapse: collapse; width: 100%; border: 1px solid #ddd;'>
                    <tr style='background:#f2f2f2;'>
                        <td style='padding: 10px; font-weight: bold; width: 40%;'>How they scheduled before ShiftEase</td>
                        <td style='padding: 10px;'>{feedback.SchedulingMethodBefore}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; font-weight: bold;'>How they found ShiftEase</td>
                        <td style='padding: 10px;'>{feedback.ReferralSource}</td>
                    </tr>
                    <tr style='background:#f2f2f2;'>
                        <td style='padding: 10px; font-weight: bold;'>Why they opened ShiftEase today</td>
                        <td style='padding: 10px;'>{feedback.TodaysPurpose}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; font-weight: bold;'>Task completed?</td>
                        <td style='padding: 10px;'>{feedback.TaskCompleted}</td>
                    </tr>
                    <tr style='background:#f2f2f2;'>
                        <td style='padding: 10px; font-weight: bold;'>What prevented them (if applicable)</td>
                        <td style='padding: 10px;'>{obstacle}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; font-weight: bold;'>Biggest challenge / what was missing</td>
                        <td style='padding: 10px;'>{feedback.BiggestChallenge}</td>
                    </tr>
                    <tr style='background:#f2f2f2;'>
                        <td style='padding: 10px; font-weight: bold;'>Rating</td>
                        <td style='padding: 10px;'><strong>{feedback.Rating}/10</strong></td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; font-weight: bold;'>Why this rating</td>
                        <td style='padding: 10px;'>{feedback.RatingReason}</td>
                    </tr>
                    <tr style='background:#f2f2f2;'>
                        <td style='padding: 10px; font-weight: bold;'>Additional comments</td>
                        <td style='padding: 10px;'>{comments}</td>
                    </tr>
                </table>
            </body>
            </html>
        ";

        await SendEmailAsync(AdminEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await client.SendMailAsync(mailMessage);
    }
}
