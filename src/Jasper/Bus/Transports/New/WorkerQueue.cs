using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.New
{
    public abstract class WorkerQueue : IWorkerQueue
    {
        private readonly CompositeLogger _logger;
        private readonly IHandlerPipeline _pipeline;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, ActionBlock<Envelope>> _receivers
            = new Dictionary<string, ActionBlock<Envelope>>();


        // TODO -- where does the cancellation token come from?
        public WorkerQueue(CompositeLogger logger, IHandlerPipeline pipeline, CancellationToken cancellationToken)
        {
            _logger = logger;
            _pipeline = pipeline;
            _cancellationToken = cancellationToken;

            // TODO -- should this be configurable?
            AddQueue(TransportConstants.Default, 5);
            AddQueue(TransportConstants.Replies, 5);
        }

        public Task Enqueue(Envelope envelope)
        {
            // TODO -- will do fancier routing later
            var receiver = _receivers.ContainsKey(envelope.Queue)
                ? _receivers[envelope.Queue]
                : _receivers[TransportConstants.Default];

            receiver.Post(envelope);

            return Task.CompletedTask;
        }

        public int QueuedCount
        {
            get
            {
                return _receivers.Values.ToArray().Sum(x => x.InputCount);
            }
        }

        public void AddQueue(string queueName, int parallelization)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = parallelization,
                CancellationToken = _cancellationToken
            };

            if (!_receivers.ContainsKey(queueName))
            {
                var receiver = new ActionBlock<Envelope>(envelope =>
                {
                    var callback = buildCallback(envelope, queueName);
                    envelope.Callback = callback;
                    envelope.ContentType = envelope.ContentType ?? "application/json";

                    return _pipeline.Invoke(envelope);
                }, options);
            }
        }

        protected abstract IMessageCallback buildCallback(Envelope envelope, string queueName);
    }
}
