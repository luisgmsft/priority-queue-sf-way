using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QueuingShared
{
    [DataContract]
    public class ExecutionData
    {
        [DataMember]
        public Guid Identifier { get; set; }
        [DataMember]
        public QueuePriority Priority { get; set; }
    }
}
