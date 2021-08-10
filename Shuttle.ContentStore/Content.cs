using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore
{
    public class Content
    {
        private byte[] _sanitizedContent;
        private readonly List<StatusEvent> _statusEvents = new List<StatusEvent>();
        private readonly Dictionary<string,string> _properties = new Dictionary<string, string>();

        public Content(Guid id, Guid referenceId, string fileName, string contentType, byte[] bytes,
            string systemName, string username, DateTime effectiveFromDate)
        {
            Guard.AgainstNullOrEmptyString(fileName, nameof(fileName));
            Guard.AgainstNullOrEmptyString(contentType, nameof(contentType));
            Guard.AgainstNull(bytes, nameof(bytes));
            Guard.AgainstNullOrEmptyString(systemName, nameof(systemName));
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            if (bytes.Length == 0)
            {
                throw new ArgumentException($"Argument '{nameof(bytes)}' may not have a zero length.");
            }

            Id = id;
            ReferenceId = referenceId;
            FileName = fileName;
            ContentType = contentType;
            Bytes = bytes;
            SystemName = systemName;
            Username = username;
            EffectiveFromDate = effectiveFromDate;
            EffectiveToDate = DateTime.MaxValue;

            OnStatusEvent(ServiceStatus.Registered, effectiveFromDate);
        }

        public DateTime StatusDateRegistered { get; private set; }

        public Guid Id { get; }
        public string FileName { get; }
        public string ContentType { get; }
        public byte[] Bytes { get; }

        public byte[] SanitizedBytes
        {
            get
            {
                if (!HasSanitizedBytes)
                {
                    throw new InvalidOperationException("The content does not contain sanitized bytes.");
                }

                return _sanitizedContent;
            }
            private set => _sanitizedContent = value;
        }

        public Content WithEffectiveToDate(DateTime effectiveToDate)
        {
            if (effectiveToDate < EffectiveFromDate)
            {
                throw new ArgumentException(
                    $"Argument '{nameof(effectiveToDate)}' with value of '{effectiveToDate:O}' may not be less than effective from date with value of '{EffectiveFromDate:O}'.");
            }

            EffectiveToDate = effectiveToDate;

            return this;
        }

        public string SystemName { get; }
        public string Username { get; }
        public ServiceStatus Status { get; private set; }
        public bool HasSanitizedBytes => _sanitizedContent != null;

        public Guid ReferenceId { get; }
        public DateTime EffectiveFromDate { get; }
        public DateTime EffectiveToDate { get; private set; }

        public Content Passed()
        {
            if (Status == ServiceStatus.Suspicious || Status == ServiceStatus.Passed)
            {
                throw new InvalidOperationException(
                    $"Cannot change status to '{ServiceStatus.Passed}' since it is already '{Status}'.");
            }

            OnStatusEvent(ServiceStatus.Passed);

            return this;
        }

        private void OnStatusEvent(ServiceStatus status)
        {
            OnStatusEvent(status, DateTime.Now);
        }

        public void OnStatusEvent(ServiceStatus status, DateTime dateRegistered)
        {
            if (status == ServiceStatus.Registered && ContainsStatus(ServiceStatus.Registered))
            {
                return;
            }

            _statusEvents.Add(new StatusEvent(status, dateRegistered));

            Status = status;
            StatusDateRegistered = dateRegistered;
        }

        public Content Suspicious()
        {
            if (Status == ServiceStatus.Suspicious || Status == ServiceStatus.Passed)
            {
                throw new InvalidOperationException(
                    $"Cannot change status to '{ServiceStatus.Suspicious}' since it is already '{Status}'.");
            }

            OnStatusEvent(ServiceStatus.Suspicious);

            return this;
        }

        public Content WithSanitizedContent(byte[] sanitizedContent)
        {
            Guard.AgainstNull(sanitizedContent, nameof(sanitizedContent));

            if (sanitizedContent.Length == 0)
            {
                throw new ArgumentException($"Argument '{nameof(sanitizedContent)}' may not have a zero length.");
            }

            SanitizedBytes = sanitizedContent;

            return this;
        }

        public bool ContainsStatus(ServiceStatus status)
        {
            return _statusEvents.Find(item => item.Status == status) != null;
        }

        private interface IStatusEvent
        {
            void MarkAdded();
        }

        public class StatusEvent : IStatusEvent
        {
            public StatusEvent(ServiceStatus status, DateTime dateRegistered)
            {
                Guard.AgainstUndefinedEnum<ServiceStatus>(status, nameof(status));

                Status = status;
                DateRegistered = dateRegistered;
            }

            public ServiceStatus Status { get; }
            public DateTime DateRegistered { get; }
            public bool Added { get; private set; }

            public void MarkAdded()
            {
                Added = true;
            }
        }

        public IEnumerable<StatusEvent> GetStatusEvents()
        {
            return _statusEvents.AsReadOnly();
        }

        public Content Processing()
        {
            if (Status != ServiceStatus.Registered)
            {
                throw new InvalidOperationException(
                    $"Cannot change status to '{ServiceStatus.Processing}' since it is already '{Status}'.  Status can only be changed when it is '{ServiceStatus.Registered}'.");
            }

            OnStatusEvent(ServiceStatus.Processing);

            return this;
        }

        public IDictionary<string, string> GetProperties()
        {
            return new ReadOnlyDictionary<string, string>(_properties);
        }

        public Content SetProperty(string name, string value)
        {
            if (ContainsProperty(name))
            {
                _properties.Remove(name);
            }

            _properties.Add(name, value);

            return this;
        }

        public bool ContainsProperty(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            return _properties.ContainsKey(name);
        }

        public string GetPropertyValue(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            if (!_properties.ContainsKey(name))
            {
                throw new InvalidOperationException($"There is no property with name '{name}'.");
            }

            return _properties[name];
        }

        public bool RemoveProperty(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            if (!_properties.ContainsKey(name))
            {
                return false;
            }

            _properties.Remove(name);

            return true;

        }
    }
}