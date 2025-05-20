using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace WackyBusinessCards.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly bool _enableSsl;
    private readonly bool _isEmailEnabled;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Load email settings from configuration
        _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.example.com";
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
        _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@wackybusinesscards.com";
        _senderName = _configuration["EmailSettings:SenderName"] ?? "Wacky Business Cards";
        _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        _isEmailEnabled = bool.Parse(_configuration["EmailSettings:Enabled"] ?? "false");
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent)
    {
        if (!_isEmailEnabled)
        {
            _logger.LogInformation("Email sending is disabled. Would have sent email to {To} with subject {Subject}", to, subject);
            return true;
        }

        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(to));

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = _enableSsl
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {To} with subject {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string to, string firstName, string lastName, string password)
    {
        var subject = "Welcome to Wacky Business Cards!";
        var htmlContent = await GetEmailTemplateAsync("WelcomeEmail", new
        {
            FirstName = firstName,
            LastName = lastName,
            Email = to,
            Password = password,
            LoginUrl = _configuration["ApplicationUrl"] + "/Identity/Account/Login",
            CurrentYear = DateTime.Now.Year.ToString()
        });

        return await SendEmailAsync(to, subject, htmlContent);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string to, string resetLink)
    {
        var subject = "Reset Your Wacky Business Cards Password";
        var htmlContent = await GetEmailTemplateAsync("PasswordReset", new
        {
            ResetLink = resetLink,
            CurrentYear = DateTime.Now.Year.ToString()
        });

        return await SendEmailAsync(to, subject, htmlContent);
    }

    public async Task<bool> SendAccountActivityNotificationAsync(string to, string activityType, string details)
    {
        var subject = $"Account Activity: {activityType}";
        var htmlContent = await GetEmailTemplateAsync("AccountActivity", new
        {
            ActivityType = activityType,
            Details = details,
            Timestamp = DateTime.Now.ToString("g"),
            CurrentYear = DateTime.Now.Year.ToString()
        });

        return await SendEmailAsync(to, subject, htmlContent);
    }

    private async Task<string> GetEmailTemplateAsync(string templateName, object model)
    {
        try
        {
            var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates", $"{templateName}.html");
            
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Email template {TemplateName} not found at {TemplatePath}", templateName, templatePath);
                return $"<p>This is a placeholder for the {templateName} template.</p>";
            }

            var template = await File.ReadAllTextAsync(templatePath);
            
            // Simple template replacement
            foreach (var prop in model.GetType().GetProperties())
            {
                var placeholder = $"{{{{{prop.Name}}}}}";
                var value = prop.GetValue(model)?.ToString() ?? string.Empty;
                template = template.Replace(placeholder, value);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email template {TemplateName}", templateName);
            return $"<p>Error processing email template {templateName}.</p>";
        }
    }
}
