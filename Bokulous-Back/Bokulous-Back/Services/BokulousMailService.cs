using Bokulous_Back.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Diagnostics;

namespace Bokulous_Back.Services
{
    public class BokulousMailService : IBokulousMailService
    {
        string _username;
        string _password;
        string _fromAddress;
        string _smptAddress;
        int _port;

        [ActivatorUtilitiesConstructor]
        public BokulousMailService(IOptions<BokulousMailSettings> bokulousMailSettings)
        {
            _username = bokulousMailSettings.Value.Username;
            _password = bokulousMailSettings.Value.Password;
            _fromAddress = bokulousMailSettings.Value.FromAddress;
            _smptAddress = bokulousMailSettings.Value.SmptAddress;
            int.TryParse(bokulousMailSettings.Value.Port, out _port);


            Debug.WriteLine("Using Mail Username: " + bokulousMailSettings.Value.Username);
        }

        public void SendEmail(string ToAddress, string subject, string body)
        {
            if (_username == "")
                return;

            using var smtp = new SmtpClient();
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_fromAddress));
            email.To.Add(MailboxAddress.Parse(ToAddress));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            Debug.WriteLine("Username: " + _username);
            Debug.WriteLine("Password: " + _password);
            Debug.WriteLine("Smpt-Address: " + _smptAddress);
            Debug.WriteLine("Port: " + _port);
            Debug.WriteLine("From: " + _fromAddress);
            Debug.WriteLine("Sending to: " + email.To.ToString());

            smtp.Connect(_smptAddress, _port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_username, _password);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
