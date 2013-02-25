using System;

namespace Edit
{
    public class ConcurrencyException : Exception
    {
        public string StreamName { get; private set; }
        public string ExpectedVersion { get; private set; }

        public ConcurrencyException(string streamName, string expectedVersion)
            : base(string.Format("Expected version {0} in stream '{1}' but it has been changed since", expectedVersion, streamName))
        {
            StreamName = streamName;
            ExpectedVersion = expectedVersion;
        }
    }
}