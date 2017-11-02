using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.New.Sending
{
    public interface ISendingAgent : IDisposable
    {
        Uri Destination { get; }

        // This would be called in the future by the outbox, assuming
        // that the envelope is already persisted and just needs to be sent out
        Task EnqueueOutgoing(Envelope envelope);

        // This would be called by the EnvelopeSender if invoked
        // indirectly
        Task StoreAndForward(Envelope envelope);

        void Start();
    }
}
