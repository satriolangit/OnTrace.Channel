using System.IO;

namespace OnTrace.Channel.Core.Interfaces
{
    public interface IFileProcessor
    {
        /// <summary>
        /// Create file and then return stream
        /// </summary>
        /// <param name="filename">path to filename</param>
        /// <returns></returns>
        FileStream CreateFileStream(string filename);

        byte[] FileStreamToBytes(string filename);

        string CurrentPath { get; }
    }
}