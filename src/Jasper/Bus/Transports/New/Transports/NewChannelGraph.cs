using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.New.Transports
{
    // TODO -- probably get rid of the IEnumerable<IChannel> thing
    public class NewChannelGraph : IChannelGraph, IDisposable
    {
        private NewBusSettings _settings;
        private readonly ConcurrentDictionary<Uri, Lazy<IChannel>> _channels = new ConcurrentDictionary<Uri, Lazy<IChannel>>();
        private readonly Dictionary<string, INewTransport> _transports = new Dictionary<string, INewTransport>();

        public void Start(NewBusSettings settings, INewTransport[] transports)
        {
            _settings = settings;

            transports.Where(x => settings.StateFor(x.Protocol) != TransportState.Disabled)
                .Each(t => _transports.Add(t.Protocol, t));




            if (settings.DefaultChannelAddress != null)
            {
                DefaultChannel = this[settings.DefaultChannelAddress];
            }

            assertNoUnknownTransportsInSubscribers(settings);


            assertNoUnknownTransportsInListeners(settings);

            foreach (var subscriberAddress in settings.KnownSubscribers)
            {
                var transport = _transports[subscriberAddress.Uri.Scheme];
                var agent = transport.BuildSendingAgent(subscriberAddress.Uri, _settings.Cancellation);

                var channel = new NewChannel(subscriberAddress, transport.DefaultReplyUri(), agent);

                _channels[subscriberAddress.Uri] = new Lazy<IChannel>(() => channel);
            }
        }

        public string[] ValidTransports => _transports.Keys.ToArray();

        // TODO -- get rid of this in favor of just using a Channels property
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IChannel> GetEnumerator()
        {
            return _channels.Values.ToArray().Select(x => x.Value).GetEnumerator();
        }

        public IChannel this[Uri uri]
        {
            get
            {
                // TODO -- make this UriLookup aware right here!!!
                assertValidTransport(uri);

                return _channels.GetOrAdd(uri, u => new Lazy<IChannel>(() => buildChannel(u))).Value;
            }
        }

        private void assertValidTransport(Uri uri)
        {
            if (!_transports.ContainsKey(uri.Scheme))
            {
                throw new ArgumentOutOfRangeException(nameof(uri), $"Unrecognized transport scheme '{uri.Scheme}'");
            }
        }

        private IChannel buildChannel(Uri uri)
        {
            assertValidTransport(uri);

            var transport = _transports[uri.Scheme];
            var agent = transport.BuildSendingAgent(uri, _settings.Cancellation);
            return new NewChannel(agent, transport.DefaultReplyUri());
        }

        public IChannel DefaultChannel { get; private set; }
        public IChannel DefaultRetryChannel => this[TransportConstants.RetryUri];
        public string Name => _settings.ServiceName;

        public void Dispose()
        {
            foreach (var value in _channels.Values)
            {
                if (value.IsValueCreated)
                {
                    value.Value.Dispose();
                }
            }

            _channels.Clear();
        }


        public IChannel TryGetChannel(Uri address)
        {
            throw new NotImplementedException();
        }

        public bool HasChannel(Uri uri)
        {
            throw new NotImplementedException();
        }

        private void assertNoUnknownTransportsInListeners(NewBusSettings settings)
        {
            var unknowns = settings.Listeners.Where(x => !ValidTransports.Contains(x.Scheme)).ToArray();

            if (unknowns.Any())
            {
                throw new UnknownTransportException(
                    $"Unknown transports referenced in listeners: {unknowns.Select(x => x.ToString()).Join(", ")}");
            }
        }

        private void assertNoUnknownTransportsInSubscribers(NewBusSettings settings)
        {
            var unknowns = settings.KnownSubscribers.Where(x => !ValidTransports.Contains(x.Uri.Scheme)).ToArray();
            if (unknowns.Length > 0)
            {
                throw new UnknownTransportException(
                    $"Unknown transports referenced in {unknowns.Select(x => x.Uri.ToString()).Join(", ")}");
            }
        }
    }
}
