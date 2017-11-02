using System;
using System.Threading;

namespace Jasper.Bus.Transports.New.Transports
{
    // Will become the new ITransport
    public interface INewTransport : IDisposable
    {
        string Protocol { get; }

        ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation);

        Uri DefaultReplyUri();

        void StartListening(NewBusSettings settings);
    }
}
