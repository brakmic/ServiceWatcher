using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using ServiceWatcherWebRole.Models;
using ServiceWatcherWebRole.Helpers;
using System.Collections.Concurrent;

namespace ServiceWatcherWebRole
{
    public class WebRole : RoleEntryPoint
    {
        public static RxStreamHelper RxHelper { get; private set; }

        static WebRole()
        {
            //initialize Rx streams
            RxHelper = new RxStreamHelper();
        }

        public override bool OnStart()
        {
            return base.OnStart();
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}
