using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Edit.AzureTableStorage
{
    public sealed class AzureTableStorageAppendOnlyStore : IAppendOnlyStore
    {
        private CloudTableClient _cloudTableClient;
        private CloudTable _cloudTable;
        private static readonly ILog Logger = LogManager.GetCurrentClassLogger();

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

        public async Task AppendAsync(string streamName, byte[] data, string expectedVersion)
        {
            await AppendAsync(streamName, data, Timeout.InfiniteTimeSpan, expectedVersion);
        }

        public async Task AppendAsync(string streamName, byte[] data, TimeSpan timeout, string expectedVersion)
        {
            await AppendAsync(streamName, data, timeout, CancellationToken.None, expectedVersion);
        }

        public async Task AppendAsync(string streamName, byte[] data, CancellationToken token, string expectedVersion)
        {
            await AppendAsync(streamName, data, Timeout.InfiniteTimeSpan, token, expectedVersion);
        }

        public async Task AppendAsync(string streamName, byte[] data, TimeSpan timeout, CancellationToken token, string expectedVersion)
        {
            bool isMissing = false;

            try
            {
                await
                    _cloudTable.ReplaceAsync(new AppendOnlyStoreTableEntity
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
                else if (e.RequestInformation.HttpStatusCode == 404)
                {
                    isMissing = true;
                }
                else
                {
                    throw;
                }
            }

            if (isMissing)
            {
                await InsertEmptyAsync(streamName, timeout, token);
                await AppendAsync(streamName, data, timeout, token, expectedVersion);
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
            bool isMissing = false;

            try
            {
                Logger.DebugFormat("BEGIN: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);
                var entity = await _cloudTable.RetrieveAsync<AppendOnlyStoreTableEntity>(streamName, RowKey);
                Logger.DebugFormat("END: Retrieve cloud table entity async id: '{0}', thread: '{1}'", streamName, Thread.CurrentThread.ManagedThreadId);

                if (entity == null)
                {
                    Logger.InfoFormat("No entity was found with stream name '{0}'", streamName);
                    isMissing = true;
                }
                else
                {
                    return new Record(entity.Data, entity.ETag);
                }                
            }
            catch (StorageException exception)
            {
                Logger.DebugFormat("ERROR: Exception {0} while retrieving cloud table entity async", exception);

                if (exception.RequestInformation.HttpStatusCode != 404)
                {
                    throw;
                }

                isMissing = true;
            }

            if (isMissing)
            {
                Logger.DebugFormat("BEGIN: Insert empty async: StreamName: '{0}'", streamName);
                await InsertEmptyAsync(streamName, timeout, token);
                Logger.DebugFormat("END: Insert empty async: StreamName: '{0}'", streamName);
            }

            return await ReadAsync(streamName, timeout, token);
        }

        private async Task InsertEmptyAsync(string streamName, TimeSpan timeout, CancellationToken token)
        {
            var entity = new AppendOnlyStoreTableEntity()
            {
                ETag = "*",
                PartitionKey = streamName,
                RowKey = RowKey,
                Data = new byte[0]
            };

            try
            {
                await _cloudTable.InsertAsync(entity);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("ERROR: Exception thrown while inserting empty on id {0}", ex, streamName);
            }
        }

        #endregion

        public void Dispose()
        {
        }
    }
}