# ServiceWatcher

Service Watcher based on <a href="https://github.com/Reactive-Extensions/Rx.NET" target="_blank">Reactive Extensions</a>

The code parts with Rx Observables are based on <a href="http://www.zerobugbuild.com/?p=230" target="_blank">this blog entry.</a>

This solution runs in Azure Emulator and receives client heartbeats via POST-requests.

We, at <a href="http://www.advarics.net" target="_blank">advarics GmbH</a>, use a similar version to control our remote Services in real-time.

The ServiceWatcher utilizes a simple Database structure following the *Code First* workflow.

Every heartbeat entry gets inserted into the table *Heartbeat* and can be received via GET.

For example to receive all Heartbeats: <a href="#">http://localhost:81/odata/Heartbeats</a>

<img src="http://p56.imgup.net/get_heartb352f.png" />

A "heartbeat" is just a simple JSON payload that describes the client and which Rx Stream it belongs to.
Rx Streams are configured via the Azure WebRole settings. There's also an option to configure the WebApp
to run as an ordinary IIS application. In this case you'd use the *Web.config* settings.

I recommend <a href="http://www.telerik.com/fiddler" target="_blank">Fiddler</a> to send Heartbeat-Requests.

<img src="http://o81.imgup.net/fiddler_pof8dd.png" />

*Heartbeats-Log in Azure Emulator*

<img src="http://s75.imgup.net/azure_emul2fa8.png" />

*The configuration of streams is done in Azure WebRole config*

<img src="http://w68.imgup.net/azure_conf566a.png" />

Streams are defined in the *RxStreams* field as comma-separated values. Every stream has its own
timeout settings which is by convention *RxStreamTimerSTREAMNAME*. 

For example: *RxStreamTimerWebShop* for
Stream named *WebShop*. If a stream has no explicit timeout set then the *RxStreamTimerDefault* will be used.

To be informed about disconnected clients (that is, clients who are not sending heartbeats within an expected time frame) you
can set up an SMTP mail-server to deliver warning notices.

**Developed at**

<a href="http://www.advarics.net/" target="_blank">advarics GmbH</a>, Innsbruck (Austria)

Branch Office Bochum (Germany)

**License**

MIT
