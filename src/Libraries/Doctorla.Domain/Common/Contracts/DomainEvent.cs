using Doctorla.Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doctorla.Domain.Common.Contracts
{
    public abstract class DomainEvent : IEvent
    {
        public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
    }
}
