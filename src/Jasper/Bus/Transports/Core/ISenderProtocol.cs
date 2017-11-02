using System.Threading.Tasks;

namespace Jasper.Bus.Transports.Core
{
    public interface ISenderProtocol
    {
        Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch);
    }
}
