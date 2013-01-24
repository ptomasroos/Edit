using System.Collections.Generic;
using ProtoBuf;

namespace Edit
{
    [ProtoContract]
    public sealed class ChunkSet
    {
        [ProtoMember(1)]
        public string Version { get; private set; }
        [ProtoMember(2)]
        public IEnumerable<Chunk> Chunks { get; private set; }

        public ChunkSet(IEnumerable<Chunk> chunks, string version)
        {
            Chunks = new List<Chunk>(chunks);
            Version = version;
        }
    }
}