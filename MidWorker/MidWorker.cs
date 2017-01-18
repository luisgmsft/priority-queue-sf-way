using System.Fabric;
using QueuingShared;

namespace MidWorker
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MidWorker : Worker
    {
        public MidWorker(StatelessServiceContext context) : base(context)
        {
            Priority = QueuePriority.Mid;
        }
    }
}
