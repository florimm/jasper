using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.New.Transports
{
    public class NewBusSettings
    {
        public TransportState StateFor(string protocol)
        {
            return TransportState.Enabled;
        }

        public IList<Uri> Listeners { get; } = new List<Uri>();


    }

    public interface INewTransport : IDisposable
    {
        string Protocol { get; }

        ISendingAgent BuildSendingAgent(Uri uri);

        Uri DefaultReplyUri();

        void StartListening(IHandlerPipeline pipeline, NewBusSettings settings);
    }

    public class TcpTransport : INewTransport
    {
        public TcpTransport(string protocol, IPersistence persistence, CompositeLogger logger)
        {
            Persistence = persistence;
            Logger = logger;
        }

        public CompositeLogger Logger { get; }

        public IPersistence Persistence { get; }

        public string Protocol { get; } = "tcp";


        public ISendingAgent BuildSendingAgent(Uri uri)
        {
            throw new NotImplementedException();
        }

        public Uri DefaultReplyUri()
        {
            throw new NotImplementedException();
        }

        public void StartListening(IHandlerPipeline pipeline, NewBusSettings settings)
        {
            if (settings.StateFor(Protocol) == TransportState.Disabled) return;

            var incoming = settings.Listeners.Where(x => x.Scheme == Protocol).ToArray();

            assertNoDuplicatePorts(incoming);

            // is it durable, or not?
        }

        // TODO -- throw a more descriptive exception
        private static void assertNoDuplicatePorts(Uri[] incoming)
        {
            var duplicatePorts = incoming.GroupBy(x => x.Port).Where(x => x.Count() > 1).ToArray();
            if (duplicatePorts.Any())
            {
                throw new Exception("You need a better exception here about duplicate ports");
            }
        }

        public void Dispose()
        {
        }
    }

}
