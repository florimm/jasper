using System;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.New.Receiving
{
    public interface IListeningAgent : IDisposable
    {
        void Start(IReceiverCallback callback);
        Uri Address { get; }
    }
}
