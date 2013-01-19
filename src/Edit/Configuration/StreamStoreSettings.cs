using Microsoft.WindowsAzure.Storage;

namespace Edit.Configuration
{
    public class StreamStoreSettings
    {
        public ISerializer Serializer { get; set; }
        public CloudStorageAccount CloudStorageAccount { get; set; }
        public string ContainerName { get; set; }
    }
}
