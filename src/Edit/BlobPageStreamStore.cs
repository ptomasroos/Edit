using Edit.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Edit
{
    public sealed class BlobPageStreamStore : IStreamStore
    {
        private readonly StreamStoreSettings _streamStoreSettings;
        private CloudBlobClient _cloudBlobClient;
        private CloudBlobContainer _cloudBlobContainer;

        public static async Task<IStreamStore> CreateStreamStoreAsync(Action<StreamStoreConfigurator> configure)
        {
            var configurator = new StreamStoreConfigurator();
            configure(configurator);

            var streamStore = new BlobPageStreamStore(configurator.Settings);
            await streamStore.StartAsync();
            return streamStore;
        }

        private async Task StartAsync()
        {
            _cloudBlobClient = _streamStoreSettings.CloudStorageAccount.CreateCloudBlobClient();
            _cloudBlobContainer = _cloudBlobClient.GetContainerReference(_streamStoreSettings.ContainerName);

            // create container if it does not exist on startup
            await _cloudBlobContainer.CreateIfNotExistAsync();
        }

        private BlobPageStreamStore(StreamStoreSettings streamStoreSettings)
        {
            _streamStoreSettings = streamStoreSettings;
        }

        public void Dispose()
        {
            // TODO
        }

        #region Append

        public Task Append(string streamName, Record[] records)
        {
            return Append(streamName, records, Timeout.InfiniteTimeSpan);
        }

        public Task Append(string streamName, Record[] records, TimeSpan timeout)
        {
            return Append(streamName, records, timeout, CancellationToken.None);
        }

        public Task Append(string streamName, Record[] records, CancellationToken token)
        {
            return Append(streamName, records, Timeout.InfiniteTimeSpan, token);
        }

        private const int _pageBlobPageSize = 512;
        private long CalculatePageOffset(long page)
        {
            return page * 512;
        }

        public async Task Append(string streamName, Record[] records, TimeSpan timeout, CancellationToken token)
        {
            var pageBlobReference = _cloudBlobContainer.GetPageBlobReference(streamName);

            // TODO remember to remove _streamStoreSettings.Serializer.Serialize(item, memoryStream); it shall be in ieventstore interface
            // TODO or records become generic <T> and hold an instance of something?

            foreach (var record in records)
            {
                try
                {
                    var data = WriteRecord(record);
                    using (var memoryStream = new MemoryStream(data))
                    {
                        pageBlobReference.FetchAttributes(); //needed for optimistic concurrency and ETAG. Sadly another http request.
                        pageBlobReference.WritePages(memoryStream, CalculatePageOffset(record.Index), accessCondition: new AccessCondition() { IfMatchETag = pageBlobReference.Properties.ETag });
                    }
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    {
                        pageBlobReference.Create(512 * _pageBlobPageSize); // this gives us 512 insertions TODO push to settings instead?
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        #endregion

        #region Read

        public Task<Record[]> Read(string streamName)
        {
            return Read(streamName, Timeout.InfiniteTimeSpan);
        }

        public Task<Record[]> Read(string streamName, TimeSpan timeout)
        {
            return Read(streamName, timeout, CancellationToken.None);
        }

        public Task<Record[]> Read(string streamName, CancellationToken token)
        {
            return Read(streamName, Timeout.InfiniteTimeSpan, token);
        }

        public async Task<Record[]> Read(string streamName, TimeSpan timeout, CancellationToken token)
        {
            var pageBlobReference = _cloudBlobContainer.GetPageBlobReference(streamName);

            var records = new List<Record>();

            using (var memoryStream = new MemoryStream())
            {
                pageBlobReference.DownloadToStream(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                int currentPage = 0;

                while (memoryStream.Length > CalculatePageOffset(currentPage))
                {
                    memoryStream.Seek(currentPage * 512, SeekOrigin.Begin);

                    var record = ReadRecord(memoryStream);
                    if (record != null)
                    {
                        records.Add(record);
                    }

                    currentPage++;
                }
            }

            return records.ToArray();
        }

        #endregion

        private byte[] WriteRecord(Record record)
        {
            using (var memoryStream = new MemoryStream(512))
            {
                using (var binary = new BinaryWriter(memoryStream))
                {
                    binary.Write(record.Index); // index
                    binary.Write(record.Bytes.Length); // length of data in int
                    binary.Write(record.Bytes); // the actual data

                    var data = new byte[memoryStream.Position];
                    memoryStream.Seek(0, SeekOrigin.Begin); //rewind stream
                    memoryStream.Read(data, 0, data.Length);

                    var hash = CalculateHash(data);
                    binary.Write(hash); // write hash to stream

                    var zeros = new byte[512 - memoryStream.Position];
                    memoryStream.Write(zeros, 0, zeros.Length);

                    return memoryStream.ToArray();
                }
            }
        }

        private Record ReadRecord(Stream source)
        {
            Record result = null;

            var binary = new BinaryReader(source);

            try
            {
                var index = binary.ReadInt64();
                var length = binary.ReadInt32();
                var bytes = binary.ReadBytes(length);

                var data = new byte[source.Position];
                source.Seek(0, SeekOrigin.Begin);
                source.Read(data, 0, data.Length);

                var actualHash = CalculateHash(data);

                var hash = binary.ReadBytes(20);

                if (!hash.SequenceEqual(actualHash))
                {
                    // TODO fix logging that this record is broken.
                }

                result = new Record(bytes, index);
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        private byte[] CalculateHash(byte[] data)
        {
            using (var sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(data);
            }
        }
    }
}