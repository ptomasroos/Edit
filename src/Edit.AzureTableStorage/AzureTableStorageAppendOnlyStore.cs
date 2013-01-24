using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public sealed class AzureTableStorageAppendOnlyStore : IAppendOnlyStore
    {
        private CloudTableClient _cloudTableClient;
        private CloudTable _cloudTable;

        private const string RowKey = "0";

        public static async Task<IAppendOnlyStore> CreateAsync(CloudStorageAccount cloudStorageAccount, string tableName)
        {
            var streamStore = new AzureTableStorageAppendOnlyStore();
            await streamStore.StartAsync(cloudStorageAccount, tableName);
            return streamStore;
        }

        private async Task StartAsync(CloudStorageAccount cloudStorageAccount, string tableName)
        {
            _cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            _cloudTable = _cloudTableClient.GetTableReference(tableName);

            // create container if it does not exist on startup
            await _cloudTable.CreateIfNotExistAsync();
        }

        #region AppendAsync

        public async Task AppendAsync(string streamName, byte[] data, string expectedVersion = null)
        {
            await AppendAsync(streamName, data, Timeout.InfiniteTimeSpan, expectedVersion);
        }

        public async Task AppendAsync(string streamName, byte[] data, TimeSpan timeout, string expectedVersion = null)
        {
            await AppendAsync(streamName, data, timeout, CancellationToken.None, expectedVersion);
        }

        public async Task AppendAsync(string streamName, byte[] data, CancellationToken token, string expectedVersion = null)
        {
            await AppendAsync(streamName, data, Timeout.InfiniteTimeSpan, token, expectedVersion);
        }

        public async Task AppendAsync(string streamName, byte[] data, TimeSpan timeout, CancellationToken token, string expectedVersion = null)
        {
            try
            {
                await
                    _cloudTable.InsertAsync(new AppendOnlyStoreTableEntity
                    {
                        PartitionKey = streamName,
                        RowKey = RowKey,
                        Data = data,
                        ETag = expectedVersion ?? "*" // "*" means that it will overwrite it and discard optimistic concurrency
                    });
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409) // 409 == Conflict
                {
                    throw new ConcurrencyException(streamName, expectedVersion);
                }

                throw;
            }
        }

        #endregion

        #region ReadAsync

        public async Task<Record> ReadAsync(string streamName)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan);
        }

        public async Task<Record> ReadAsync(string streamName, TimeSpan timeout)
        {
            return await ReadAsync(streamName, timeout, CancellationToken.None);
        }

        public async Task<Record> ReadAsync(string streamName, CancellationToken token)
        {
            return await ReadAsync(streamName, Timeout.InfiniteTimeSpan, token);
        }

        public async Task<Record> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token)
        {
            var entity = await _cloudTable.RetrieveAsync<AppendOnlyStoreTableEntity>(streamName, RowKey);
            return new Record(entity.Data, entity.ETag);
        }

        #endregion

        public void Dispose()
        {
        }
    }
}