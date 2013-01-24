using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public sealed class AppendOnlyStoreTableEntity : TableEntity
    {
        public byte[] Data { get; set; }
    }
}