namespace OnTrace.Channel.Core.Entities
{
    public class InboundFileLog
    {
        public InboundFileLog()
        {
            IsAttachment = true;
        }

        public int FileId { get; set; }
        public string LogId { get; set; }
        public string Filename { get; set; }
        public string FileType { get; set; }
        public string Url { get; set; }
        public bool IsAttachment { get; set; }
        public byte[] FileData { get; set; }
    }
}