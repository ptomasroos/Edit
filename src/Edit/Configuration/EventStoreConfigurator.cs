namespace Edit.Configuration
{
    public class EventStoreConfigurator
    {
        internal EventStoreSettings Settings { get; private set; }

        internal EventStoreConfigurator()
        {
            Settings = new EventStoreSettings();
        }

        public void WithSerializer(ISerializer serializer)
        {
            Settings.Serializer = serializer;
        }

        public void WithAppendOnlyStore(IAppendOnlyStore appendOnlyStore)
        {
            Settings.AppendOnlyStore = appendOnlyStore;
        }
    }
}
