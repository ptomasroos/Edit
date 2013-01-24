using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Edit.Configuration;

namespace Edit
{
    public sealed class StreamStore : IStreamStore
    {
        private readonly EventStoreSettings _settings;
        private readonly Framer _framer;

        public static IStreamStore Create(Action<EventStoreConfigurator> configure)
        {
            var configurator = new EventStoreConfigurator();
            configure(configurator);

            return new StreamStore(configurator.Settings);
        }

        private StreamStore(EventStoreSettings settings)
        {
            _settings = settings;
            _framer = new Framer(_settings.Serializer);
        }

        #region AppendAsync

        public async Task AppendAsync(string streamName, IEnumerable<Chunk> events, string expectedVersion = null)
        {
            await AppendAsync(streamName, events, Timeout.InfiniteTimeSpan, expectedVersion);
        }

        public async Task AppendAsync(string streamName, IEnumerable<Chunk> events, TimeSpan timeout, string expectedVersion = null)
        {
            await AppendAsync(streamName, events, timeout, CancellationToken.None, expectedVersion);
        }

        public async Task AppendAsync(string streamName, IEnumerable<Chunk> events, CancellationToken token, string expectedVersion = null)
        {
            await AppendAsync(streamName, events, Timeout.InfiniteTimeSpan, token, expectedVersion);
        }

        public async Task AppendAsync(string streamName, IEnumerable<Chunk> events, TimeSpan timeout, CancellationToken token, string expectedVersion = null)
        {
            byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                foreach (var e in events)
                {
                    var result = _framer.Write(e);
                    memoryStream.Write(result, 0, result.Length);
                }

                data = memoryStream.ToArray();
            }

            await _settings.AppendOnlyStore.AppendAsync(streamName, data, timeout, token, expectedVersion);
        }

        #endregion


        #region ReadAsync

        public async Task<ChunkSet> ReadAsync(string streamName)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout)
        {
            return await ReadAsync(streamName, timeout, CancellationToken.None);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, CancellationToken token)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan, token);
        }

        public async Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token)
        {
            var record = await _settings.AppendOnlyStore.ReadAsync(streamName, timeout, token);

            using (var memoryStream = new MemoryStream(record.Data))
            {
                var chunks = _framer.Read<Chunk>(memoryStream);
                return new ChunkSet(chunks, record.StreamVersion);
            }
        }

        #endregion
    }
}