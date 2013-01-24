namespace Edit.Configuration
{
    public class EventStoreSettings
    {
        public IAppendOnlyStore AppendOnlyStore { get; set; }
        public ISerializer Serializer { get; set; }
    }
}
