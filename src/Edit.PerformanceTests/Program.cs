using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Edit.AzureTableStorage;
using Edit.Protobuf;
using Microsoft.WindowsAzure.Storage;
using ProtoBuf;

namespace Edit.PerformanceTests
{
    public class Program
    {
        private static readonly List<Guid> _ids = new List<Guid>();

        const int NumberOfInsertions = 1000;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting performance tests");

            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 1000;

            var p = new Program();
            p.Run().Wait();

            Console.WriteLine("Finished performance tests");
            Console.Read();
        }

        public async Task Run()
        {
            var eventStore = await WireupEventStoreAsync();
            var stopWatch = new Stopwatch();

            Console.WriteLine("Running {0} insertions", NumberOfInsertions);
            stopWatch.Start();

            var tasks = new ConcurrentQueue<Task>();

            for (var i = 0; i < NumberOfInsertions; i++)
            {
                var e = new CreatedCustomer(Guid.NewGuid(), "Edit");

                var task = eventStore.AppendAsync(e.Id.ToString(), new List<Chunk>()
                    {
                        new Chunk() {Instance = e}
                    }, null);
                tasks.Enqueue(task);

                _ids.Add(e.Id);
            }

            await Task.WhenAll(tasks);
            stopWatch.Stop();

            Console.WriteLine("Time elapsed {0} seconds", stopWatch.Elapsed.TotalSeconds);
            Console.WriteLine("{0} writes per second in average", NumberOfInsertions / stopWatch.Elapsed.TotalSeconds);

            stopWatch.Reset();
            tasks = new ConcurrentQueue<Task>();

            Console.WriteLine("Running {0} reads", NumberOfInsertions);
            stopWatch.Start();

            foreach (var id in _ids)
            {
                var task = eventStore.ReadAsync(id.ToString());
                tasks.Enqueue(task);
            }

            await Task.WhenAll(tasks);
            stopWatch.Stop();

            Console.WriteLine("Time elapsed {0} seconds", stopWatch.Elapsed.TotalSeconds);
            Console.WriteLine("{0} reads per second in average", NumberOfInsertions / stopWatch.Elapsed.TotalSeconds);
        }

        private async Task<IStreamStore> WireupEventStoreAsync()
        {
            var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;

            var tableStore = await AzureTableStorageAppendOnlyStore.CreateAsync(cloudStorageAccount, "performancetests");
            return StreamStore.Create(configure =>
                {
                    configure.WithAppendOnlyStore(tableStore);
                    configure.WithProtobufSerialization();
                });
        }
    }

    [ProtoContract]
    public class CreatedCustomer
    {
        [ProtoMember(1)]
        public Guid Id { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }

        public CreatedCustomer()
        {
            
        }

        public CreatedCustomer(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
