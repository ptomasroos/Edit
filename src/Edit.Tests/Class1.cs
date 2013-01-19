using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Edit.Protobuf;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;

namespace Edit.Tests
{
    public class Class1
    {
        [Fact]
        public async void Test()
        {
            //WriteToPageBlob(new Uri(@"http://127.0.0.1:10000/"), "devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==");
            /*
             *  Account name: devstoreaccount1
                Account key: Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==
             * */
            CloudStorageAccount account;
            CloudStorageAccount.TryParse("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://ipv4.fiddler",
                                         out account);

            //Blob Service: http://127.0.0.1:10000/<account-name>/<resource-path>
            var streamStore = await BlobPageStreamStore.CreateStreamStoreAsync(configurator =>
                {
                    configurator.WithContainerName("test");
                    configurator.WithCloudStorageAccount(account);
                    configurator.WithSerializer<StreamStoreProtobufSerializer>();
                });



            var exampleMessages = new[] { new ExampleMessage() }; //,new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage(),new ExampleMessage() };

            var records = new List<Record>();
            var serializer = new StreamStoreProtobufSerializer();
            for (int i = 0; i < exampleMessages.Length; i++)
            {
                                var message = exampleMessages[i];

                byte[] data;
                using (var memoryStream = new MemoryStream())
                {
                    serializer.Serialize(message, memoryStream);
                    data = memoryStream.ToArray();
                }
                var record = new Record(data, i);
                records.Add(record);
            }
            await streamStore.Append("apa", records.ToArray(), Timeout.InfiniteTimeSpan, CancellationToken.None);

            var readrecords = await streamStore.Read("apa");

            //ManualResetEventSlim evt = new ManualResetEventSlim();
            //bus.Subscribe<string>(async m => evt.Set()).Wait();
            //bus.Publish("testing").Wait();
            //evt.Wait();
            //Thread.Sleep(5000);
        }
    }
}
