using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IEventStore : IDisposable
    {
        Task Append(string streamName, IEvent[] events);
        Task Append(string streamName, IEvent[] events, TimeSpan timeout);
        Task Append(string streamName, IEvent[] events, CancellationToken token);
        Task Append(string streamName, IEvent[] events, TimeSpan timeout, CancellationToken token);

        Task<IEvent[]> Read(string streamName);
        Task<IEvent[]> Read(string streamName, TimeSpan timeout);
        Task<IEvent[]> Read(string streamName, CancellationToken token);
        Task<IEvent[]> Read(string streamName, TimeSpan timeout, CancellationToken token);
    }
}