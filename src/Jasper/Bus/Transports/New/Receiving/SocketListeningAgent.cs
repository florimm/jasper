﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Transports.Core;
using Jasper.Util;

namespace Jasper.Bus.Transports.New.Receiving
{
    public class SocketListeningAgent : IListeningAgent
    {
        private readonly int _port;
        private readonly CancellationToken _cancellationToken;
        private TcpListener _listener;
        private ActionBlock<Socket> _socketHandling;

        public SocketListeningAgent(int port, CancellationToken cancellationToken)
        {
            _port = port;
            _cancellationToken = cancellationToken;


            Address = $"tcp://{Environment.MachineName}:{port}/".ToUri();
        }

        public void Start(IReceiverCallback callback)
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, _port));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socketHandling = new ActionBlock<Socket>(async s =>
            {
                using (var stream = new NetworkStream(s, true))
                {
                    await WireProtocol.Receive(stream, callback, Address);
                }
            });
        }

        public Uri Address { get; }

        public void Dispose()
        {
            _socketHandling?.Complete();
            _listener?.Stop();
            _listener?.Server.Dispose();
        }
    }
}