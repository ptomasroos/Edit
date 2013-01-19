using Edit.Configuration;

namespace Edit.Protobuf
{
    public static class ProtobufSerializationConfigurator
    {
        public static void WithProtobufSerialization(this StreamStoreConfigurator configurator)
        {
            configurator.WithSerializer(new StreamStoreProtobufSerializer());
        }
    }
}
