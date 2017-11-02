using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.New.Sending;

namespace Jasper.Bus.Transports.New.Transports.Stub
{
    public class StubChannel : ISendingAgent, IDisposable
    {
        private readonly IHandlerPipeline _pipeline;
        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public StubChannel(Uri destination, IHandlerPipeline pipeline)
        {
            Destination = destination;
        }

        public void Dispose()
        {

        }

        public Uri Destination { get; }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            var callback = new StubMessageCallback(this);
            Callbacks.Add(callback);

            envelope.Callback = callback;

            envelope.ReceivedAt = Destination;

            return _pipeline.Invoke(envelope);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public void Start()
        {

        }
    }
}