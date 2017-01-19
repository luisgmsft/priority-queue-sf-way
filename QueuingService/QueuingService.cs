using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using QueuingShared;
using System.Linq;
using Microsoft.ServiceFabric.Data.Collections.Preview;

namespace QueuingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class QueuingService : StatefulService, IQueuing, IWorker
    {
        private IReliableConcurrentQueue<ExecutionData> lowQueue;
        private IReliableConcurrentQueue<ExecutionData> midQueue;
        private IReliableConcurrentQueue<ExecutionData> topQueue;
        private bool initialised;

        public QueuingService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task Enqueue(ExecutionData data)
        {
            using (var tx = this.StateManager.CreateTransaction())
            {
                switch (data.Priority)
                {
                    case QueuePriority.Low:
                        await lowQueue.EnqueueAsync(tx, data);
                        break;
                    case QueuePriority.Mid:
                        await midQueue.EnqueueAsync(tx, data);
                        break;
                    case QueuePriority.Top:
                        await topQueue.EnqueueAsync(tx, data);
                        break;
                }
                
                await tx.CommitAsync();
            }
        }

        public async Task<ExecutionData> GetNext(QueuePriority priority)
        {
            ExecutionData result = null;

            if (!initialised)
                return null;

            using (var tx = this.StateManager.CreateTransaction())
            {
                switch (priority)
                {
                    case QueuePriority.Low:
                        result = await lowQueue.DequeueAsync(tx);
                        break;
                    case QueuePriority.Mid:
                        result = await midQueue.DequeueAsync(tx);
                        break;
                    case QueuePriority.Top:
                        result = await topQueue.DequeueAsync(tx);
                        break;
                }

                await tx.CommitAsync();
            }

            return result;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(context =>
                this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            lowQueue = await this.StateManager.GetOrAddAsync<IReliableConcurrentQueue<ExecutionData>>("low-priority");
            midQueue = await this.StateManager.GetOrAddAsync<IReliableConcurrentQueue<ExecutionData>>("mid-priority");
            topQueue = await this.StateManager.GetOrAddAsync<IReliableConcurrentQueue<ExecutionData>>("top-priority");

            initialised = true;

            var random = new Random();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Enumerable.Range(0, 100)
                .Select(i =>
                {
                    var target = random.Next(0, 3);
                    var data = new ExecutionData
                    {
                        Identifier = Guid.NewGuid(),
                        Priority = (QueuePriority)target
                    };
                    return data;
                }).ToList()
                .ForEach(async t => {
                    await Enqueue(t);
                });

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }
}
