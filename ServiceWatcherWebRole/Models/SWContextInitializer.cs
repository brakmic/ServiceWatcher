using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ServiceWatcherWebRole.Models
{
    /// <summary>
    /// Code first initialization
    /// </summary>
    public class SWContextInitializer : DropCreateDatabaseAlways<SWContext>
    {
        /// <summary>
        /// A few predefined service watcher entries
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(SWContext context)
        {
            Random rnd = new Random();
            List<Heartbeat> heartbeats = new List<Heartbeat>();
            for (int i = 0; i < 10; i++)
            {
                int next = rnd.Next(1,100);
                Heartbeat hb = new Heartbeat
                {
                    AppName = "WorkerInstance_" + i.ToString(),
                    CustomerId = next,
                    InstanceId = Guid.NewGuid(),
                    StreamName = (next % 2 == 0) ? "Account" : "WebShop",
                    CustomerName = (next % 2 == 0) ? "Custome_X" : "Customer_Y",
                    AppVersion = "1.0." + i.ToString(),
                    CreatedAt = DateTimeOffset.Now,
                    IsDeleted = false
                };
                heartbeats.Add(hb);
            }
            context.Heartbeats.AddRange(heartbeats);
            base.Seed(context);
        }
    }
}