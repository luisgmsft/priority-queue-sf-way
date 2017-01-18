using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueuingShared
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public class Worker : StatelessService
    {
        protected QueuePriority Priority { get; set; }

        public Worker(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var serviceUri = new Uri("fabric:/PriorityHttpGateway/QueuingService");

            IWorker service = null;
            try
            {
                while (service == null)
                {
                    service = ServiceProxy.Create<IWorker>(serviceUri, new ServicePartitionKey(1));
                    if (service == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    }
                }

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var data = await service.GetNext(Priority);

                    if (data != null)
                    {
                        if (data.Priority == this.Priority)
                        {
                            ServiceEventSource.Current.ServiceMessage(this.Context, "Working on {0} from {1} queue.", data.Identifier, data.Priority.ToString());
                        }
                        else
                        {
                            ServiceEventSource.Current.ServiceMessage(this.Context, "Skipping {0} because it belongs to {1} queue.", data.Identifier, data.Priority.ToString());
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }
    }
}
