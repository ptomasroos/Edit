using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edit.Protobuf;
using ProtoBuf;
using Xunit;

namespace Edit.Tests
{
    public interface IMessage
    {
        Guid Id { get; set; }
    }

    public interface IEvent : IMessage
    {
    }

    [ProtoContract]
    public class ExampleMessage : IEvent
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }
        [ProtoMember(2)]
        public Guid Id2 { get; set; }
        [ProtoMember(3)]
        public Guid Id3 { get; set; }
        [ProtoMember(4)]
        public Guid Id4 { get; set; }
        [ProtoMember(5)]
        public Guid Id5 { get; set; }

        public ExampleMessage()
        {
            Id = Guid.NewGuid();
            Id2 = Guid.NewGuid();
            Id3 = Guid.NewGuid();
            Id4 = Guid.NewGuid();
            Id5 = Guid.NewGuid();
        }
    }

    public class ProtobufSerializerTest
    {
        [Fact]
        public void HowLarge()
        {
            var stream = new MemoryStream();
            var exampleMessage = new ExampleMessage();

            var streamStoreProtobufSerializer = new StreamStoreProtobufSerializer();
            streamStoreProtobufSerializer.Serialize(exampleMessage, stream);

            var array = stream.ToArray();
            Debug.Write("The event takes " + array.Length + "bytes of storage");
        }
    }
}
