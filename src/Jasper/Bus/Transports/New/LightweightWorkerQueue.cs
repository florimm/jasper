using System.Threading;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.New
{
    public interface ILightweightWorkerQueue : IWorkerQueue{}

    public class LightweightWorkerQueue : WorkerQueue, ILightweightWorkerQueue
    {
        public LightweightWorkerQueue(CompositeLogger logger, IHandlerPipeline pipeline, CancellationToken cancellationToken) : base(logger, pipeline, cancellationToken)
        {
        }

        protected override IMessageCallback buildCallback(Envelope envelope, string queueName)
        {
            return new LightweightCallback(this);
        }
    }
}
