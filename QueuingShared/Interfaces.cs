using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueuingShared
{
    public interface IWorker : IService
    {
        Task<ExecutionData> GetNext(QueuePriority priority);
    }
    public interface IQueuing : IService
    {
        Task Enqueue(ExecutionData data);
    }
}
