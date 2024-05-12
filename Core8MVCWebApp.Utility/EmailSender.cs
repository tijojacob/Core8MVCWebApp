﻿using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core8MVC.Utility
{
    public class EmailSender : IEmailSender
    {
        public string SendGridSecret { get; set; }
        public EmailSender(IConfiguration _config)
        {
            SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //email logic
            var client = new SendGridClient(SendGridSecret);
            var frm = new EmailAddress("admin@app.com", "test email");
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(frm, to, subject, "", htmlMessage);
            return client.SendEmailAsync(message);
            //throw new NotImplementedException();
        }
    }
}
