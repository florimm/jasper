using System;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.New.Sending;

namespace Jasper.Bus.Transports.New.Transports.Stub
{
    public class StubTransport : INewTransport
    {
        public readonly LightweightCache<Uri, StubChannel> Channels;
        private readonly IHandlerPipeline _pipeline;
        private Uri _replyUri;

        public StubTransport(IHandlerPipeline pipeline)

        {
            _pipeline = pipeline;
            _replyUri = new Uri($"stub://replies");
            
            Channels =
                new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri, pipeline));


        }

        public void Dispose()
        {

        }

        public string Protocol { get; } = "stub";
        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            return Channels[uri];
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public void StartListening(NewBusSettings settings)
        {
            var incoming = settings.Listeners.Where(x => x.Scheme == "stub");
            foreach (var uri in incoming)
            {
                Channels.FillDefault(incoming);
            }
        }
    }
}