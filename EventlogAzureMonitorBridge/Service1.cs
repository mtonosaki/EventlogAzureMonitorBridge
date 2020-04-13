// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tono;

namespace EventlogAzureMonitorBridge
{
    public partial class Service1 : ServiceBase
    {
        private Dictionary<string, string> Params;
        private readonly CancellationTokenSource CancelHandler = new CancellationTokenSource();
        private readonly Queue<EventlogMessageEventArgs> Messages = new Queue<EventlogMessageEventArgs>();

        public Service1()
        {
            InitializeComponent();
        }

        private AzureUploader uploader = null;
        private EventlogListener listener = null;

        protected override void OnStart(string[] args)
        {
            args = Environment.GetCommandLineArgs();
            if (ParseArgs(args))
            {
                eventlog($"Start EventlogAzureMonitorBridge service.");
            }
            else
            {
                var mes = $"Starting Error Service1. Need to set the all parameters. /n=LogName /p=PortNo /w=WorkspaceID of Azure LogAnalitycs /k=Key1";
                eventlog(mes, true);
                new Timer(prm =>
                {
                    Stop(); // Delay Service Stop when error.  (otherwise, Service Stop --> Start in Event log)
                }, null, 1000, Timeout.Infinite);
                return;
            }

            // Prepare Event Log Listener
            listener = new EventlogListener
            {
                StatePath = Path.GetDirectoryName(GetType().Assembly.Location),
            };
            listener.OnMessage += Listener_OnMessage;
            listener.OnError += Listener_OnError;
            var task1 = listener.RunAsync(CancelHandler.Token);


            // Prepare Azure Monitor Http Uploader
            uploader = new AzureUploader
            {
                LogName = Params["/n"],
                WorkspaceID = Params["/w"],
                Key1 = Params["/k"],
                Messages = () => Messages,
                Logger = eventlog,
            };
            var task2 = uploader.PorlingMessagesAsync(CancelHandler.Token);
        }

        private void Listener_OnError(object sender, EventlogErrorEventArgs e)
        {
            eventlog(e.Exception.Message);
            Stop();
        }

        private void Listener_OnMessage(object sender, EventlogMessageEventArgs e)
        {
            lock (Messages)
            {
                Messages.Enqueue(e);
            }
        }

        public void OnStartConsole(string[] args)
        {
            OnStart(args);

            for (; ; )
            {
                eventlog("To stop debugging, press stop button of Visual Studio");
                Task.Delay(10000).Wait();
            }
        }

        /// <summary>
        /// Parse command line
        /// </summary>
        /// <param name="args"></param>
        /// <returns>true=OK / false = Insufficient parameter setting.</returns>
        private bool ParseArgs(string[] args)
        {
            var prms = new[] { "/w", "/k", "/n" };
            Params = prms.ToDictionary(a => a);

            foreach (var arg in args)
            {
                foreach (var prm in prms)
                {
                    var key = prm + "=";
                    if (arg.StartsWith(key))
                    {
                        Params[prm] = StrUtil.MidSkip(arg, key);
                        break;
                    }
                }
            }
            foreach (var kv in Params)
            {
                if (kv.Key == kv.Value)
                {
                    return false;
                }
            }
            return true;
        }

        protected override void OnStop()
        {
            CancelHandler.Cancel();
            eventlog($"Stopped Service1");
        }

        public void OnStopConsole()
        {
        }

        private void eventlog(string mes, bool isError = false)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(mes);
            }
            else
            {
                eventLog1.WriteEntry(mes, isError ? EventLogEntryType.Error : EventLogEntryType.Information);
            }
        }
    }
}
