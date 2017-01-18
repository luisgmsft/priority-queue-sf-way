using System.Fabric;
using QueuingShared;

namespace LowWorker
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class LowWorker : Worker
    {
        public LowWorker(StatelessServiceContext context) : base(context)
        {
            Priority = QueuePriority.Low;
        }
    }
}
