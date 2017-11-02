﻿using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Loopback;

namespace Jasper.Util
{
    public static class UriExtensions
    {
        public const string Durable = "durable";

        public static string QueueName(this Uri uri)
        {
            if (uri == null) return null;

            if (uri.Scheme == LoopbackTransport.ProtocolName && uri.Host != Durable)
            {
                return uri.Host;
            }

            var lastSegment = uri.Segments.Skip(1).LastOrDefault();
            if (lastSegment == Durable) return TransportConstants.Default;

            return lastSegment ?? TransportConstants.Default;
        }

        private static readonly HashSet<string> _locals = new HashSet<string>(new[] { "localhost", "127.0.0.1" }, StringComparer.OrdinalIgnoreCase);

        public static bool IsDurable(this Uri uri)
        {
            if (uri.Scheme == LoopbackTransport.ProtocolName && uri.Host == Durable) return true;

            var firstSegment = uri.Segments.Skip(1).FirstOrDefault();
            if (firstSegment == null) return false;

            return Durable == firstSegment.TrimEnd('/');
        }

        public static Uri ToMachineUri(this Uri uri)
        {
            return _locals.Contains(uri.Host) ? uri.ToLocalUri() : uri;
        }

        public static Uri ToLocalUri(this Uri uri)
        {
            return new UriBuilder(uri) { Host = Environment.MachineName }.Uri;
        }
    }
}
