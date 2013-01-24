using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public interface IStreamStore
    {
        Task AppendAsync(string streamName, IEnumerable<Chunk> chunks, string expectedVersion = null);
        Task AppendAsync(string streamName, IEnumerable<Chunk> chunks, TimeSpan timeout, string expectedVersion = null);
        Task AppendAsync(string streamName, IEnumerable<Chunk> chunks, CancellationToken token, string expectedVersion = null);
        Task AppendAsync(string streamName, IEnumerable<Chunk> chunks, TimeSpan timeout, CancellationToken token, string expectedVersion = null);

        Task<ChunkSet> ReadAsync(string streamName);
        Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout);
        Task<ChunkSet> ReadAsync(string streamName, CancellationToken token);
        Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token);
    }
}