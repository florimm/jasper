﻿using System.Diagnostics;

namespace Jasper.Messaging.Runtime
{
    public partial class Envelope
    {
        private Stopwatch _stopwatch;

        internal void StartTiming()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        internal void MarkCompletion(bool success)
        {
            _stopwatch.Stop();

            ExecutionDuration = _stopwatch.ElapsedMilliseconds;
            _stopwatch = null;

            Succeeded = success;
        }

        /// <summary>
        /// Did the envelope succeed during execution?
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Tracks the execution duration in milliseconds in the handling
        /// pipeline
        /// </summary>
        public long ExecutionDuration { get; private set; }
    }
}
