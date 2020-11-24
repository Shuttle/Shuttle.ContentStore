using System;
using System.Data;
using Shuttle.Core.Data;

namespace Shuttle.ContentStore
{
    public class Columns
    {
        public static readonly MappedColumn<byte[]> Content = new MappedColumn<byte[]>("Content", DbType.Binary);
        public static MappedColumn<string> ContentType = new MappedColumn<string>("ContentType", DbType.AnsiString);
        public static readonly MappedColumn<DateTime> DateRegistered = new MappedColumn<DateTime>("DateRegistered", DbType.DateTime2);
        public static MappedColumn<Guid> DocumentId = new MappedColumn<Guid>("DocumentId", DbType.Guid);
        public static MappedColumn<string> FileName = new MappedColumn<string>("FileName", DbType.AnsiString);
        public static MappedColumn<Guid> Id = new MappedColumn<Guid>("Id", DbType.Guid);
        public static MappedColumn<string> Name = new MappedColumn<string>("Name", DbType.AnsiString);
        public static readonly MappedColumn<DateTime> EffectiveFromDate = new MappedColumn<DateTime>("EffectiveFromDate", DbType.DateTime2);
        public static readonly MappedColumn<DateTime> EffectiveToDate = new MappedColumn<DateTime>("EffectiveToDate", DbType.DateTime2);
        public static MappedColumn<Guid> ReferenceId = new MappedColumn<Guid>("ReferenceId", DbType.Guid);
        public static readonly MappedColumn<byte[]> SanitizedContent = new MappedColumn<byte[]>("SanitizedContent", DbType.Binary);
        public static readonly MappedColumn<int> SequenceNumber = new MappedColumn<int>("SequenceNumber", DbType.Int32);
        public static MappedColumn<string> Status = new MappedColumn<string>("Status", DbType.AnsiString);
        public static readonly MappedColumn<DateTime> StatusDateRegistered = new MappedColumn<DateTime>("StatusDateRegistered", DbType.DateTime2);
        public static MappedColumn<string> SystemName = new MappedColumn<string>("SystemName", DbType.AnsiString);
        public static MappedColumn<string> Username = new MappedColumn<string>("Username", DbType.AnsiString);
        public static MappedColumn<string> Value = new MappedColumn<string>("Value", DbType.AnsiString);
    }
}