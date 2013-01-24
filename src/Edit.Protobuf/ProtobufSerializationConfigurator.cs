using Edit.Configuration;

namespace Edit.Protobuf
{
    public static class ProtobufSerializationConfigurator
    {
        public static void WithProtobufSerialization(this EventStoreConfigurator configurator)
        {
            configurator.WithSerializer(new ProtobufSerializer());
        }
    }
}
