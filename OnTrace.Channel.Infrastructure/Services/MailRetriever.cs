using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Pop3;
using MimeKit;
using OnTrace.Channel.Core.Domain;
using System.Text.RegularExpressions;


namespace OnTrace.Channel.Infrastructure.Services
{
    public class MailRetriever
    {
        private readonly MailAccount _mailAccount;
        private readonly FileProcessor _fileProcessor;
        
        public MailRetriever(MailAccount mailAccount, FileProcessor fileProcessor)
        {
            _mailAccount = mailAccount;
            _fileProcessor = fileProcessor;
        }

        public ICollection<MailMessage> GetMailMessages()
        {
            List<MailMessage> result = new List<MailMessage>();

            using (var client = new Pop3Client())
            {
                // Connect to mail
                client.Connect(_mailAccount.Server, _mailAccount.Port, _mailAccount.EnableSsl);

                // Note: since we don't have an OAuth2 token, disable
                // the XOAUTH2 authentication mechanism.
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_mailAccount.Username, _mailAccount.Password);

                return GetMailMessages(client.Count);
            }

            
        }

        public ICollection<MailMessage> GetMailMessages(int limit)
        {
            List<MailMessage> result = new List<MailMessage>();
            try
            {
                using (var client = new Pop3Client())
                {
                    // Connect to mail
                    client.Connect(_mailAccount.Server, _mailAccount.Port, _mailAccount.EnableSsl);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_mailAccount.Username, _mailAccount.Password);

                    for (int i = 0; i < client.Count; i++)
                    {
                        var message = client.GetMessage(i);

                        // Get medias or attachments
                        var medias = GetMediaFiles(message);

                        var from = message.From.Mailboxes.FirstOrDefault();
                        
                        if (from != null && !Regex.IsMatch(from.Address, "[+-]"))
                        {
                            var mailMessage = new MailMessage()
                            {
                                From = from.Address,
                                To = _mailAccount.Username,
                                Message = message.TextBody,
                                MediaFiles = medias,
                                Subject = message.Subject
                                
                            };

                            result.Add(mailMessage);
                        }
                    }
                    client.Disconnect(true);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve mail messages", ex);
            }
           
            return result;
        }

        public List<MediaFile> GetMediaFiles(MimeMessage message)
        {
            var files = new List<MediaFile>();
            try
            {
                using (var iter = new MimeIterator(message))
                {
                    // collect our list of attachments and their parent multiparts
                    while (iter.MoveNext())
                    {
                        var multipart = iter.Parent as Multipart;
                        var part = iter.Current as MimePart;

                        if (multipart == null || part == null) continue;
                        if (part.FileName == null) continue;

                        //save media
                        string filePath = $"{_fileProcessor.CurrentPath}/files/{part.FileName}";
                        _fileProcessor.SaveMedia(filePath, part);

                        var file = new MediaFile
                        {
                            Filename = part.FileName,
                            IsAttachment = part.IsAttachment,
                            FileData = _fileProcessor.StreamToBytes(filePath)
                        };
                        files.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw new Exception($"Failed to retrieve mail attachment or medias", ex);
            }
            

            return files;
        }
    }
}
