namespace Shuttle.ContentStore.DataAccess.Query
{
    public class RawContent
    {
        public string Status { get; set; }
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}