using System.Fabric;
using QueuingShared;

namespace TopWorker
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class TopWorker : Worker
    {
        public TopWorker(StatelessServiceContext context) : base(context)
        {
            Priority = QueuePriority.Top;
        }
    }
}
