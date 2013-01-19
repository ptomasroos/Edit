using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IStreamStore : IDisposable
    {
        Task Append(string streamName, Record[] records);
        Task Append(string streamName, Record[] records, TimeSpan timeout);
        Task Append(string streamName, Record[] records, CancellationToken token);
        Task Append(string streamName, Record[] records, TimeSpan timeout, CancellationToken token);

        Task<Record[]> Read(string streamName);
        Task<Record[]> Read(string streamName, TimeSpan timeout);
        Task<Record[]> Read(string streamName, CancellationToken token);
        Task<Record[]> Read(string streamName, TimeSpan timeout, CancellationToken token);
    }

}
