namespace Edit
{
    public sealed class Record
    {
        public byte[] Data { get; private set; }
        public string StreamVersion { get; private set; }

        public Record(byte[] data, string streamVersion)
        {
            Data = data;
            StreamVersion = streamVersion;
        }
    }
}