using ServiceWatcherWebRole.Events;
using ServiceWatcherWebRole.Helpers;
using ServiceWatcherWebRole.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;

namespace ServiceWatcherWebRole.Controllers
{
    /// <summary>
    /// Delegate for propagating heartbeat events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void HeartBeatDelegate(object sender, HeartbeatEventArgs e);

    public abstract class BaseODataController : ODataController
    {
        protected SWContext db;
        /// <summary>
        /// Ctor (the db context instance will be injected via Ninject)
        /// Consult NinjectWebCommon.cs in App_Start
        /// </summary>
        /// <param name="ctx"></param>
        public BaseODataController(SWContext ctx)
        {
            this.db = ctx;
        }

    }
}
