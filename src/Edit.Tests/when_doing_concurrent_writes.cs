using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.Storage;

namespace Edit.Tests
{
    public class when_doing_concurrent_writes
    {
        private Establish context = () =>
            {
                eventStore = Bootstrapper.WireupEventStore();

                streamName = Guid.NewGuid().ToString();
                eventStore.AppendAsync(streamName, new List<Chunk>(), null).Wait();


                // two readers which will be out of sync
                var chunkset1 = eventStore.ReadAsync(streamName).Result;
                chunkset2 = eventStore.ReadAsync(streamName).Result;

                // new write operation
                eventStore.AppendAsync(streamName, chunkset1.Chunks, chunkset1.Version).Wait();
            };

        private Because of = () =>
            {
                exception = Catch.Exception(() =>
                    eventStore.AppendAsync(streamName, chunkset2.Chunks, chunkset2.Version).Wait());
            };

        private It should_have_an_exception = () =>
            {
                exception.ShouldNotBeNull();
            };

        private It should_have_an_precondition_failed_exception = () =>
            {
                var aggregateException = exception as AggregateException;
                var innerException = aggregateException.InnerExceptions.First() as StorageException;
                innerException.RequestInformation.HttpStatusCode.ShouldEqual(412);
            };

        protected static IStreamStore eventStore;
        protected static Exception exception;
        protected static string streamName;
        protected static ChunkSet chunkset2;
    }
}
