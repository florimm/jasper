using System;
using System.Collections.Generic;
using System.Threading;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.Transports.New.Transports
{
    public class NewBusSettings
    {
        public TransportState StateFor(string protocol)
        {
            return TransportState.Enabled;
        }

        // Ugh, will need to keep track of aliases in the UriLookup too. #sadtrombone
        public IList<Uri> Listeners { get; } = new List<Uri>();


        public CancellationToken Cancellation => _cancellation.Token;

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        public void StopAll()
        {
            _cancellation.Cancel();
        }

        // Duplicate, won't transfer over

        public string ServiceName { get; set; }
        public Uri DefaultChannelAddress { get; set; }
        public readonly IList<SubscriberAddress> KnownSubscribers = new List<SubscriberAddress>();
    }
}
