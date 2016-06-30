using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Cryptography;
using OnTrace.Channel.Core.Domain;
using OnTrace.Channel.Infrastructure.Data;

namespace OnTrace.Channel.Infrastructure.Services
{
    public class MailSender
    {
        private readonly MailAccount _mailAccount;
        private readonly FileProcessor _fileProcessor;
        
        public MailSender(MailAccount mailAccount, FileProcessor fileProcessor)
        {
            _mailAccount = mailAccount;
            _fileProcessor = fileProcessor;
        }
        public void SendEmail(string to, string subject, string textMessage,
            List<MailAttachment> attachments)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(_mailAccount.Server, _mailAccount.Port, SecureSocketOptions.SslOnConnect);
                    client.Authenticate(_mailAccount.Username, _mailAccount.Password);

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("", _mailAccount.Username));
                    message.To.Add(new MailboxAddress("", to));
                    message.Subject = subject;

                    var body = new TextPart("plain") { Text = textMessage };

                    var multipart = new Multipart("mixed");

                    foreach (var file in attachments)
                    {
                        var attachment = new MimePart
                        {
                            ContentObject = new ContentObject(_fileProcessor.OpenRead(file.Path)),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = file.Filename
                        };

                        multipart.Add(attachment);
                    }

                    multipart.Add(body);
                    message.Body = multipart;

                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Sending mail failed, to=[{to}], subject=[{subject}] ", ex);
            }
            
            
            
        }
    }
}
