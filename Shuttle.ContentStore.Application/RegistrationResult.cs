using System;

namespace Shuttle.ContentStore.Application
{
    public class RegistrationResult
    {
        public Guid Id { get; }
        public DateTime EffectiveFromDate { get; }

        public RegistrationResult(Guid id, DateTime effectiveFromDate)
        {
            Id = id;
            EffectiveFromDate = effectiveFromDate;
        }
    }
}