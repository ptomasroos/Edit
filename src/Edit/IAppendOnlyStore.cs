using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IAppendOnlyStore : IDisposable
    {
        Task AppendAsync(string streamName, byte[] data, string expectedVersion);
        Task AppendAsync(string streamName, byte[] data, TimeSpan timeout, string expectedVersion);
        Task AppendAsync(string streamName, byte[] data, CancellationToken token, string expectedVersion);
        Task AppendAsync(string streamName, byte[] data, TimeSpan timeout, CancellationToken token, string expectedVersion);

        Task<Record> ReadAsync(string streamName);
        Task<Record> ReadAsync(string streamName, TimeSpan timeout);
        Task<Record> ReadAsync(string streamName, CancellationToken token);
        Task<Record> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token);
    }
}
