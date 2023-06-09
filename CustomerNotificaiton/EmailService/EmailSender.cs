﻿using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _emailConfig;

        public EmailSender(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendAsync(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.From, _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            BodyBuilder bodyBuilder = new BodyBuilder
            {
                HtmlBody = string.Format("<h2 style='color:red;>{0}</h2>", message.Content)
            };
            if (message.Attachements != null && message.Attachements.Any())
            {
                int i = 0;
                foreach(var attachment in message.Attachements)
                {
                    bodyBuilder.Attachments.Add("attachment" + i, attachment);
                    i++;
                }
            }
            emailMessage.Body = bodyBuilder.ToMessageBody();

            return emailMessage;

        }

        private async Task SendAsync(MimeMessage emailMessage)
        {
            using(var client = new SmtpClient())
            {
                try
                {
                    return;
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, false);
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);   
                    await client.SendAsync(emailMessage);
    
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                    throw;
                }
                finally 
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
