

namespace Microsoft.Samples.ServiceBus.Connections
{
    using System;
    using System.ServiceModel;

    public class StreamServerHost : ServiceHost
    {
        InputQueue<StreamConnection> availableStreams;

        public StreamServerHost(params Uri[] baseAddresses)
            : base(typeof(StreamServer), baseAddresses)
        {
            availableStreams = new InputQueue<StreamConnection>();
        }

        public InputQueue<StreamConnection> AvailableStreams
        {
            get
            {
                return availableStreams;
            }
        }

        public StreamConnection Accept()
        {
            return availableStreams.Dequeue(TimeSpan.MaxValue);
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return availableStreams.BeginDequeue(TimeSpan.MaxValue, callback, state);
        }

        public StreamConnection EndAccept(IAsyncResult asyncResult)
        {
            return availableStreams.EndDequeue(asyncResult);
        }
    }
}
