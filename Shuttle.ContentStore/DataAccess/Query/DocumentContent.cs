namespace Shuttle.ContentStore.DataAccess.Query
{
    public class DocumentContent
    {
        public string Status { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}