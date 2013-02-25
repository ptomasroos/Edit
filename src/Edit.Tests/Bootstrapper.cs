using Edit.AzureTableStorage;
using Edit.Protobuf;
using Microsoft.WindowsAzure.Storage;

namespace Edit.Tests
{
    public class Bootstrapper
    {
        public static IStreamStore WireupEventStore()
        {
            var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;

            var tableStore = AzureTableStorageAppendOnlyStore.CreateAsync(cloudStorageAccount, "assumptions").Result;
            return StreamStore.Create(configure =>
            {
                configure.WithAppendOnlyStore(tableStore);
                configure.WithProtobufSerialization();
            });
        }
    }
}
