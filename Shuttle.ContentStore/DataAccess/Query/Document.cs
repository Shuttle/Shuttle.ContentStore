using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.ContentStore.DataAccess.Query
{
    public class Document
    {
        public Guid Id { get; set; }
        public Guid ReferenceId { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime EffectiveToDate { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string SystemName { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public DateTime StatusDateRegistered { get; set; }

        public List<Property> Properties { get; set; } = new List<Property>();
        public List<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();

        public class Specification
        {
            private readonly List<Guid> _ids = new List<Guid>();

            public int MaximumRows { get; private set; }
            public bool StatusEventsIncluded { get; private set; }
            public bool PropertiesIncluded { get; private set; }
            public bool ActiveOnly { get; private set; }

            public bool HasIds => _ids.Any();

            public Specification GetMaximumRows(int maximumRows)
            {
                MaximumRows = maximumRows;

                return this;
            }

            public Specification IncludeStatusEvents()
            {
                StatusEventsIncluded = true;

                return this;
            }

            public Specification IncludeProperties()
            {
                PropertiesIncluded = true;

                return this;
            }

            public Specification AddId(Guid id)
            {
                _ids.Add(id);

                return this;
            }

            public IEnumerable<Guid> GetIds()
            {
                return _ids.AsReadOnly();
            }

            public Specification GetActiveOnly()
            {
                if (!HasIds)
                {
                    throw new InvalidOperationException("Can only return active entries when there is at least one id specified.");
                }

                ActiveOnly = true;

                return this;
            }
        }

        public class Property
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class StatusEvent
        {
            public string Status { get; set; }
            public DateTime DateRegistered { get; set; }
        }
    }
}