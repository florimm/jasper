using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.New.Sending
{
    public interface ISender : IDisposable
    {
        void Start(ISenderCallback callback);

        Task Enqueue(Envelope envelope);
        Uri Destination { get; }

        int QueuedCount { get; }
    }
}
