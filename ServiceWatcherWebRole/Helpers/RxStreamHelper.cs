using ServiceWatcherWebRole.Events;
using ServiceWatcherWebRole.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Web;

namespace ServiceWatcherWebRole.Helpers
{
    /// <summary>
    /// Helper class for maintaining the Rx streams
    /// All config is done in the Azure WebRole config
    /// Every stream can have its own timeout set by using the convention RxStreamTimeout*STREAM-NAME*
    /// For example: RxStreamTimeoutWebShop
    /// Streams without timeouts get the default one from RxStreamTimeoutDefault (30 seconds)
    /// New RxStreams can also be defined via the WebRole config. The setting RxStreams contains a comma-separated
    /// list of available streams.
    /// A client must send its heartbeats via POST-requests (Content-Type: json) 
    /// against the url: SERVER/odata/Hearbeats. Each heartbeat 
    /// The JSON payload must contain the name of one of the registered streams. 
    /// For example:
    /// <code>
    ///     {
    ///         "CustomerId": 10,
    ///         "CustomerName": "Customer_X",
    ///         "InstanceId": "a02fc4f7-b96a-4ab5-a91a-cb98d36a2917",
    ///         "StreamName": "WebShop",
    ///         "AppName": "BackgroundWorker_8",
    ///         "AppVersion": "1.0.8"
    ///     } 
    /// </code>
    /// When a client has failed to send a heartbeat within the expected time frame a warning-email can be sent. 
    /// The SMTP server config & email-addresses must be set into the appropriate fields within the WebRole config. 
    /// </summary>
    public class RxStreamHelper : IDisposable
    {
        /// <summary>
        /// Rx Subject instance (more info: https://msdn.microsoft.com/en-us/library/hh242970%28v=vs.103%29.aspx)
        /// </summary>
        public ConcurrentDictionary<string, Subject<Heartbeat>> ObservedClients { get; private set; }
        /// <summary>
        /// default timeout
        /// </summary>
        private TimeSpan defaultTimeToHold = TimeSpan.FromSeconds(30);
        /// <summary>
        /// timers for registered Rx Streams
        /// </summary>
        private ConcurrentDictionary<string, TimeSpan> streamTimers;
        /// <summary>
        /// Observable collection of expired clients
        /// </summary>
        private ConcurrentDictionary<string, IObservable<Heartbeat>> expireds = null;
        /// <summary>
        /// Rx Subscriptions for each of the expired clients Observable collections (IDisposables)
        /// </summary>
        private ConcurrentDictionary<string, IDisposable> subscriptions = null;
        private string[] streamNames;
        private string rxStreamTimerPrefix = "RxStreamTimer";

        public RxStreamHelper()
        {
            ObservedClients = new ConcurrentDictionary<string, Subject<Heartbeat>>();
            InitStreams();
        }

        private void InitStreams()
        {
            //get config settings from WebRole config (alternatively, you can use Web.conf if the WebApp is running
            //as an ordinary IIS process)
            LoadConfiguration();
            //initialize Rx streams
            InitRx();
        }

        private void LoadConfiguration()
        {
            LoadStreamSettings();
            LoadTimerSettings();
        }
        /// <summary>
        /// Initi Rx streams
        /// </summary>
        private void InitRx()
        {

            expireds = new ConcurrentDictionary<string, IObservable<Heartbeat>>();
            subscriptions = new ConcurrentDictionary<string, IDisposable>();
            foreach (var stream in streamNames)
            {
                TimeSpan timer = GetTimespanForStream(stream);
                var clients = new Subject<Heartbeat>();
                ObservedClients.TryAdd(stream, clients);
                // LINQ against the clients Observable
                // group clients by the Stream name + throttling the following events in each group 
                // by the predefined timer value (after the first event in a certain group all following events will
                // be ignored). After the timeout has been reached the group will be recreated when a new element with
                // the same group Id arrives.
                var expired = clients.Synchronize().GroupByUntil(beat => beat.CustomerId,
                                                                group => group.Throttle(timer))
                                                                .SelectMany(group => group.TakeLast(1));
                expireds.TryAdd(stream, expired);

                var subscription = expired.Subscribe<Heartbeat>(OnClientDisconnect,
                                                     ex => AdvaricsHelper.Log("Error: " + ex.Message, "Warning"),
                                                     () => AdvaricsHelper.Log("Completed."));
                subscriptions.TryAdd(stream, subscription);
                AdvaricsHelper.Log(string.Format("Initialized subscription for stream {0}", stream));
            }
        }
        /// <summary>
        /// Get individual timespans for streams, or return the default value
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private TimeSpan GetTimespanForStream(string stream)
        {
            TimeSpan timerValue;
            if (!streamTimers.TryGetValue(stream, out timerValue))
            {
                timerValue = defaultTimeToHold;
            }
            return timerValue;
        }

        /// <summary>
        /// Get Rx stream names from config
        /// </summary>
        private void LoadStreamSettings()
        {
            streamNames = AdvaricsHelper.GetConfigurationSettingValue("RxStreams").Split(new[] { ',' });
        }
        /// <summary>
        /// Get timer values from config
        /// </summary>
        private void LoadTimerSettings()
        {
            streamTimers = new ConcurrentDictionary<string, TimeSpan>();
            int defaultTimeToHold = int.Parse(AdvaricsHelper.GetConfigurationSettingValue("RxStreamTimerDefault"));
            foreach (var streamName in streamNames)
            {
                string value = null;
                try
                {
                    value = AdvaricsHelper.GetConfigurationSettingValue(rxStreamTimerPrefix + streamName);
                    streamTimers.TryAdd(streamName, TimeSpan.FromSeconds(int.Parse(value)));
                }
                catch
                {
                    streamTimers.TryAdd(streamName, TimeSpan.FromSeconds(defaultTimeToHold));
                }
                AdvaricsHelper.Log(string.Format(
                                                "Setting {0} timer to {1} seconds", streamName, streamTimers[streamName].Seconds.ToString()));
            }
        }
        /// <summary>
        /// This method will be called each time a client misses to send a heartbeat within the predefined time frame
        /// Technically this means when a new client has been inserted into the expireds-Observable collection
        /// </summary>
        /// <param name="beat"></param>
        private void OnClientDisconnect(Heartbeat beat)
        {
            AdvaricsHelper.Log(
                string.Format("Disconnected Client {0} in {1} at {2}",
                                                            beat.InstanceId.ToString(),
                                                            beat.StreamName,
                                                            beat.CustomerName));
            string subject = string.Format("Disconnect at customer: {0}", beat.CustomerName);
            string message = string.Format("Client Id: {0}\r\nCustomer: {1}\r\nStream: {2}",
                                                        beat.InstanceId, beat.CustomerName, beat.StreamName);
            AdvaricsHelper.SendEmail(subject, message);
        }

        public static void OnHeartBeat(object sender, HeartbeatEventArgs e)
        {
            Heartbeat beat = new Heartbeat
            {
                AppName = e.AppName,
                AppVersion = e.AppVersion,
                CreatedAt = e.CreatedAt,
                CustomerId = e.CustomerId,
                CustomerName = e.CustomerName,
                StreamName = e.StreamName,
                Id = e.HeartbeatId,
                InstanceId = e.InstanceId
            };
            WebRole.RxHelper.ObservedClients[e.StreamName].OnNext(beat);
            AdvaricsHelper.Log(
                string.Format("Heartbeat from Client {0} in {1} at {2}",
                                                            beat.InstanceId.ToString(),
                                                            beat.StreamName,
                                                            beat.CustomerName));
        }

        public static HeartbeatEventArgs GetHeartbeatEventArgs(Heartbeat heartbeat)
        {
            return new HeartbeatEventArgs(heartbeat.CustomerId, heartbeat.CustomerName,
                                                        heartbeat.StreamName, heartbeat.InstanceId,
                                                        heartbeat.Id, heartbeat.AppName,
                                                        heartbeat.AppVersion);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (subscriptions != null)
                {
                    foreach (var subscr in subscriptions.Values)
                    {
                        Trace.WriteLine("Closing subscription.");
                        subscr.Dispose();
                    }
                }
            }
        }
    }
}