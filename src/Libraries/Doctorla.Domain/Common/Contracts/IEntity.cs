using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doctorla.Domain.Common.Contracts
{
    public interface IEntity
    {
        List<DomainEvent> DomainEvents { get; }
    }

    public interface IEntity<TId> : IEntity
    {
        TId Id { get; }
    }
}
