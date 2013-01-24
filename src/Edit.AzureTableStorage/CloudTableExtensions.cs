using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Edit.AzureTableStorage
{
    internal static class CloudTableExtensions
    {
        private static readonly RetryPolicy RetryPolicy =
            new RetryPolicy<StorageTransientErrorDetectionStrategy>(new ExponentialBackoff("Retry exponentially",
                                                                                           int.MaxValue,
                                                                                           TimeSpan.FromMilliseconds(10),
                                                                                           TimeSpan.FromSeconds(2),
                                                                                           TimeSpan.FromMilliseconds(30),
                                                                                           true));

        public static async Task<bool> CreateIfNotExistAsync(this CloudTable cloudTable)
        {
            return await RetryPolicy.ExecuteAsync(() => Task.Factory.FromAsync<bool>(cloudTable.BeginCreateIfNotExists,
                                                                                     cloudTable.EndCreateIfNotExists,
                                                                                     null));
        }

        public static async Task<TableResult> ExecuteAsync(this CloudTable cloudTable, TableOperation tableOperation)
        {
            return await RetryPolicy.ExecuteAsync(() =>
                               Task<TableResult>.Factory.FromAsync(cloudTable.BeginExecute, cloudTable.EndExecute, tableOperation, null));
        }

        public static async Task<T> RetrieveAsync<T>(this CloudTable cloudTable, string partitionKey, string rowKey) where T : class, ITableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var task = await cloudTable.ExecuteAsync(retrieveOperation);
            return task.Result as T;
        }

        public static async Task InsertAsync<T>(this CloudTable cloudTable, T tableEntity) where T : class, ITableEntity
        {
            var insertOperation = TableOperation.InsertOrReplace(tableEntity);
            await cloudTable.ExecuteAsync(insertOperation);
        }
    }
}