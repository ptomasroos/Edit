using Microsoft.WindowsAzure.Storage;

namespace Edit.Configuration
{
    public class StreamStoreConfigurator
    {
        internal StreamStoreSettings Settings { get; private set; }

        internal StreamStoreConfigurator()
        {
            Settings = new StreamStoreSettings();
        }

        public void WithContainerName(string containerName)
        {
            Settings.ContainerName = containerName;
        }

        public void WithCloudStorageAccount(CloudStorageAccount cloudStorageAccount)
        {
            Settings.CloudStorageAccount = cloudStorageAccount;
        }

        public void WithSerializer<T>() where T : ISerializer, new()
        {
            Settings.Serializer = new T();
        }

        public void WithSerializer(ISerializer serializer)
        {
            Settings.Serializer = serializer;
        }
    }
}
