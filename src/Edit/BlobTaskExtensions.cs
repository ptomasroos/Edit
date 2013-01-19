using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Edit
{
    internal static class BlobTaskExtensions
    {
        private static readonly RetryPolicy RetryPolicy = new RetryPolicy<TransientErrorDetectionStrategy>(new ExponentialBackoff("Retry exponentially",
                                                                                                                                  int.MaxValue, TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(30), true));

        public static async Task<bool> CreateIfNotExistAsync(this CloudBlobContainer cloudBlobContainer)
        {
            try
            {
                return await RetryPolicy.ExecuteAsync(() => Task.Factory.FromAsync<bool>(cloudBlobContainer.BeginCreateIfNotExists,
                                                                                         cloudBlobContainer.EndCreateIfNotExists, null)).WithTimeoutAndCancellation(TimeSpan.FromSeconds(5), CancellationToken.None);
            }
            catch (StorageException e)
            {
                // container is being deleted
                // TODO check whats wrong and whats not. They changed their schema.

                throw;
            }
        }

        public static async void DownloadToStreamAsync(this CloudPageBlob cloudPageBlob, Stream target)
        {
            var blobRequestOptions = new BlobRequestOptions();
            await Task.Factory.FromAsync(cloudPageBlob.BeginDownloadToStream, cloudPageBlob.EndDownloadToStream, target, blobRequestOptions);
        }
    }
}