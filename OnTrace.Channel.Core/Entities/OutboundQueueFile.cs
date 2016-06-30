namespace OnTrace.Channel.Core.Entities
{
    public class OutboundQueueFile
    {
        public int FileID { get; set; }
        public string QueueID { get; set; }
        public string Filename { get; set; }
        public string FileType { get; set; }
        public string Url { get; set; }
        public bool IsAttachment { get; set; }
        public byte[] FileData { get; set; }
    }
}
