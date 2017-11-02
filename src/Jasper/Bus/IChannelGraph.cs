using System;
using System.Collections.Generic;

namespace Jasper.Bus
{

    public interface IChannelGraph : IEnumerable<IChannel>
    {
        IChannel this[Uri uri] { get; }

        IChannel DefaultChannel { get; }
        IChannel DefaultRetryChannel { get; }

        string Name { get; }

        [Obsolete("This one might be unnecessary by always building a channel")]
        string[] ValidTransports { get;}

        [Obsolete("Make this unnecessary by always building a channel")]
        IChannel TryGetChannel(Uri address);

        [Obsolete("Make this unnecessary by always building a channel")]
        bool HasChannel(Uri uri);
    }
}
