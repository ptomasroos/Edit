using System;
using System.IO;
using ProtoBuf;

namespace Edit.Protobuf
{
    public class ProtobufSerializer : ISerializer
    {
        public void Serialize<T>(T instance, Stream target) where T : class
        {
            // ProtoBuf must have non-zero files
            target.WriteByte(42);

            Serializer.Serialize(target, instance);
        }

        public T Deserialize<T>(Stream source)
        {
            CheckSignature(source);

            return Serializer.Deserialize<T>(source);
        }

        public object Deserialize(Type type, Stream source)
        {
            CheckSignature(source);

            return Serializer.NonGeneric.Deserialize(type, source);
        }

        private void CheckSignature(Stream stream)
        {
            var signature = stream.ReadByte();

            if (signature != 42)
                throw new InvalidOperationException("Unknown stream for protobuf");
        }
    }
}