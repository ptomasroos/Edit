using System;
using System.Collections.Generic;
using Machine.Specifications;

namespace Edit.Tests
{
    public class when_writing_sequentially
    {
        private Establish context = () =>
            {
                eventStore = Bootstrapper.WireupEventStore();

                streamName = Guid.NewGuid().ToString();
                eventStore.AppendAsync(streamName, new List<Chunk>(), null).Wait();


                // two readers which will be out of sync
                var chunkset1 = eventStore.ReadAsync(streamName).Result;

                // new write operation
                eventStore.AppendAsync(streamName, chunkset1.Chunks, chunkset1.Version).Wait();
            };

        private Because of = () =>
            {
                var chunkset2 = eventStore.ReadAsync(streamName).Result;
                eventStore.AppendAsync(streamName, chunkset2.Chunks, chunkset2.Version).Wait();
                worked = true;
            };

        private It should_have_worked = () =>
            {
                worked.ShouldBeTrue();
            };

        protected static IStreamStore eventStore;
        protected static string streamName;
        protected static bool worked;
    }
}
