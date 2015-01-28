using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceWatcherWebRole.Events
{
    /// <summary>
    /// Heartbeat event args
    /// </summary>
    public class HeartbeatEventArgs : EventArgs
    {
        public int CustomerId { get; private set; }
        public string CustomerName { get; private set; }
        public string StreamName { get; private set; }
        public Guid HeartbeatId { get; private set; }
        public string AppName { get; private set; }
        public string AppVersion { get; private set; }
        public Guid InstanceId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public HeartbeatEventArgs(int customerId, string customerName, 
                                            string streamName, Guid instanceId, 
                                            Guid heartbeatId, string appName, 
                                            string appVersion)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            StreamName = streamName;
            HeartbeatId = heartbeatId;
            AppName = appName;
            AppVersion = appVersion;
            InstanceId = instanceId;
            CreatedAt = DateTimeOffset.Now;
        }
    }
}