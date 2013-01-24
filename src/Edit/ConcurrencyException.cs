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

    //public class OptimisticConcurrencyException : Exception TODO FIX ME!
    //{
    //    public long ActualVersion { get; private set; }
    //    public long ExpectedVersion { get; private set; }
    //    public IList<IEvent> ActualEvents { get; private set; }

    //    OptimisticConcurrencyException(string message, long actualVersion, long expectedVersion, IIdentity id,
    //        IList<IEvent> serverEvents)
    //        : base(message)
    //    {
    //        ActualVersion = actualVersion;
    //        ExpectedVersion = expectedVersion;
    //        Id = id;
    //        ActualEvents = serverEvents;
    //    }

    //    public static OptimisticConcurrencyException Create(long actual, long expected, IIdentity id,
    //        IList<IEvent> serverEvents)
    //    {
    //        var message = string.Format("Expected v{0} but found v{1} in stream '{2}'", expected, actual, id);
    //        return new OptimisticConcurrencyException(message, actual, expected, id, serverEvents);
    //    }

    //    protected OptimisticConcurrencyException(
    //        SerializationInfo info,
    //        StreamingContext context)
    //        : base(info, context) { }
    //}
}