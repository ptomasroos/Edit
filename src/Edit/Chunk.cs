using ProtoBuf;

namespace Edit
{
    [ProtoContract]
    public sealed class Chunk
    {
        [ProtoMember(1, DynamicType = true)]
        public object Instance { get; set; }
    }
}


