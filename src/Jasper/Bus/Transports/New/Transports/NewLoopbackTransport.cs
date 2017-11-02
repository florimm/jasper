using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.New.Sending;
using Jasper.Util;

namespace Jasper.Bus.Transports.New.Transports
{
    public class NewLoopbackTransport : INewTransport
    {
        private readonly ILightweightWorkerQueue _lightweightWorkerQueue;
        private readonly IDurableWorkerQueue _workerQueue;
        private readonly IPersistence _persistence;

        public NewLoopbackTransport(ILightweightWorkerQueue lightweightWorkerQueue, IDurableWorkerQueue workerQueue, IPersistence persistence)
        {
            _lightweightWorkerQueue = lightweightWorkerQueue;
            _workerQueue = workerQueue;
            _persistence = persistence;
        }

        public void Dispose()
        {
            // Nothing really
        }

        public string Protocol => "loopback";

        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            return uri.IsDurable()
                ? (ISendingAgent) new DurableSendingAgent(uri, this)
                : new LightweightSendingAgent(uri, this);
        }

        public Uri DefaultReplyUri()
        {
            return TransportConstants.RetryUri;
        }

        public void StartListening(NewBusSettings settings)
        {
            // Nothing really, since it's just a handoff to the internal worker queues
        }

        private class LightweightSendingAgent : ISendingAgent
        {
            private readonly NewLoopbackTransport _parent;
            public Uri Destination { get; }

            public LightweightSendingAgent(Uri destination, NewLoopbackTransport parent)
            {
                _parent = parent;
                Destination = destination;
            }

            public void Dispose()
            {
                // Nothing
            }

            public Task EnqueueOutgoing(Envelope envelope)
            {
                return _parent._lightweightWorkerQueue.Enqueue(envelope);
            }

            public Task StoreAndForward(Envelope envelope)
            {
                return EnqueueOutgoing(envelope);
            }

            public void Start()
            {
                // nothing
            }
        }

        private class DurableSendingAgent : ISendingAgent
        {
            private readonly NewLoopbackTransport _parent;
            public Uri Destination { get; }

            public DurableSendingAgent(Uri destination, NewLoopbackTransport parent)
            {
                _parent = parent;
                Destination = destination;
            }

            public void Dispose()
            {
                // nothing
            }

            public Task EnqueueOutgoing(Envelope envelope)
            {
                return _parent._workerQueue.Enqueue(envelope);
            }

            public Task StoreAndForward(Envelope envelope)
            {
                // TODO -- go async, and get an overload w/ a single envelope
                // please.
                _parent._persistence.StoreInitial(new Envelope[]{envelope});

                return EnqueueOutgoing(envelope);
            }

            public void Start()
            {
                // nothing
            }
        }
    }
}
