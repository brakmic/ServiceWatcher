# ServiceWatcher

Service Watcher based on <a href="https://github.com/Reactive-Extensions/Rx.NET" target="_blank">Reactive Extensions</a>

The code part that deals with Rx Observables is based on <a href="http://www.zerobugbuild.com/?p=230" target="_blank">this blog entry.</a>

This solution runs in Azure Emulator and receives client heartbeats via POST-calls.

I recommend <a href="http://www.telerik.com/fiddler" target="_blank">Fiddler</a> to send Heartbeat-Requests.

<img src="http://r01.imgup.net/fiddler_po7053.png" />

A "heartbeat" is just a simple JSON payload that describes the client and which Rx Stream it belongs to.
Rx Streams are configured via the Azure WebRole settings. There's also an option to configure the WebApp
to run as an ordinary IIS application. In this case you'd use the Web.conf settings. 

*This is how the heartbeats are being logged in the Azure Emulator*

<img src="http://s75.imgup.net/azure_emul2fa8.png" />

*The configuration of streams is done in Azure WebRole config*

<img src="http://w68.imgup.net/azure_conf566a.png" />

Streams are defined in 'RxStreams' field as comma-separated values. Every stream has its own
timeout settings which is defined by convention 'RxStreamTimer*STREAMNAME*'. For example: RxStreamTimerWebShop for
Stream named 'WebShop'. If a stream has no explicit timeout set then the RxStreamTimerDefault will be used.

To be informed about disconnected clients (that is, clients who are not sending heartbeats within an expected time frame) you
can set up an SMTP mail-server to deliver warning notices.

**License**

MIT