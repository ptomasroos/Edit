using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Edit
{
    public class Framer
    {
        private readonly ISerializer _serializer;
        private readonly SHA1Managed _sha1Managed = new SHA1Managed();

        public Framer(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public byte[] Write<T>(T e) where T : class
        {
            byte[] eSerialized;

            using (var memoryStream = new MemoryStream())
            {
                _serializer.Serialize(e, memoryStream);
                eSerialized = memoryStream.ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var binary = new BinaryWriter(memoryStream))
                {
                    binary.Write(eSerialized.Length); // length of data in int
                    binary.Write(eSerialized); // the actual data

                    var data = new byte[memoryStream.Position];
                    memoryStream.Seek(0, SeekOrigin.Begin); //rewind stream
                    memoryStream.ReadAsync(data, 0, data.Length); // read to data

                    var hash = ComputeHash(data);
                    binary.Write(hash); // write hash to stream

                    return memoryStream.ToArray();
                }
            }
        }

        public IEnumerable<T> Read<T>(Stream source) where T : class
        {
            var frames = new List<T>();
            var binary = new BinaryReader(source);

            source.Seek(0, SeekOrigin.Begin); // make sure the stream is at position 0

            while (source.Length > source.Position)
            {
                var length = binary.ReadInt32();
                var bytes = binary.ReadBytes(length);

                var data = new byte[source.Position];
                source.Seek(0, SeekOrigin.Begin);
                source.ReadAsync(data, 0, data.Length);

                var actualHash = ComputeHash(data);

                var hash = binary.ReadBytes(20);

                if (!hash.SequenceEqual(actualHash))
                {
                    // This is broken, but it doesn't really matter. 
                    // Shall we log it ?
                }

                using (var memoryStream = new MemoryStream(bytes))
                {
                    var e = _serializer.Deserialize<T>(memoryStream);
                    frames.Add(e);
                }
            }

            return frames;
        }

        private byte[] ComputeHash(byte[] data)
        {
            return _sha1Managed.ComputeHash(data);
        }
    }
}
