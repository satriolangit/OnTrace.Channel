using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using OnTrace.Channel.Core.Interfaces;


namespace OnTrace.Channel.Infrastructure.Services
{
    public class FileProcessor
    {
        public void SaveMedia(string path, MimePart media)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (var stream = File.Create(path))
                    {
                        media.ContentObject.DecodeTo(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save media, path=[{path}]", ex);
            }
            
        }

        public void DownloadAndWriteMedia(string url, string pathToWrite)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, pathToWrite);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download and write media. [from : {url}]");
            }
        }

        public Byte[] StreamToBytes(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert stream to bytes, path=[{path}]", ex);
            }
            
        }

        public string CurrentPath
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public FileStream OpenRead(string path)
        {
            try
            {
                return File.OpenRead(path);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to open and read file, path=[{path}]", ex);
            }
            
        }

        public void BytesToFile(string path, byte[] bytes)
        {
            try
            {
                if (!File.Exists(path)) File.WriteAllBytes(path, bytes);
            }
            catch (IOException ex)
            {
                throw new Exception($"Failed to convert bytes to file, path=[{path}]", ex);
            }
        }

        public string GetFilename(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }
     

    }
}
