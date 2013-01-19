namespace Edit
{
    public sealed class Record
    {
        public byte[] Bytes { get; private set; }
        public long Index { get; private set; }

        public Record(byte[] bytes, long index)
        {
            Bytes = bytes;
            Index = index;
        }
    }
}