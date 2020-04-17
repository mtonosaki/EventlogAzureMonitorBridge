// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tono;

namespace EventlogAzureMonitorBridge
{
    public class EventlogListener
    {
        public event EventHandler<EventlogMessageEventArgs> OnMessage;
        public event EventHandler<EventlogErrorEventArgs> OnError;
        public string StatePath { set; private get; }

        /// <summary>
        /// Listen Windows Eventlog message
        /// </summary>
        /// <param name="cancellationToken"></param>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(1327, cancellationToken);

            try
            {
                for (; cancellationToken.IsCancellationRequested == false; )
                {
                    var nMessage = 0;
                    var nError = 0;
                    var eventLogs = EventLog.GetEventLogs();
                    LoadState();
                    foreach (var log in eventLogs)
                    {
                        foreach (var e in GetEventLog(log, cancellationToken).OrderBy(a => a.Index))
                        {
                            try
                            {
                                var item = new EventlogMessageEventArgs
                                {
                                    EventUtcTime = e.TimeGenerated.ToUniversalTime(),
                                    LogName = log.Log,
                                    Source = e.Source,
                                    EventID = e.InstanceId,
                                    EventRecordID = e.Index,
                                    User = e.UserName,
                                    Computer = e.MachineName,
                                    Message = ReplaceMessage(e.Message),
                                };
                                OnMessage?.Invoke(this, item);
                                nMessage++;
                            }
                            catch (Exception ex)
                            {
                                OnError?.Invoke(this, new EventlogErrorEventArgs
                                {
                                    Exception = ex,
                                });
                                nError++;
                            }
                        }
                        if (cancellationToken.IsCancellationRequested) break;
                    }
                    SaveState();
#if DEBUG
                    Console.WriteLine($"Message = {nMessage},  Error = {nError}");
                    await Task.Delay(1327, cancellationToken);
#else
                    await Task.Delay(10331, cancellationToken);
#endif
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new EventlogErrorEventArgs
                {
                    Exception = ex,
                });
            }
        }

        static readonly List<(string From, string To)> ConvTable = new List<(string From, string To)>
        {
            ("%%1832" ,"Identification"),
            ("%%1833" ,"Impersonation"),
            ("%%1840" ,"Delegation"),
            ("%%1841" ,"Denied by Process Trust Label ACE"),
            ("%%1842" ,"Yes"),
            ("%%1843" ,"No"),
            ("%%1844" ,"System"),
            ("%%1845" ,"Not Available"),
            ("%%1846" ,"Default"),
            ("%%1847" ,"DisallowMmConfig"),
            ("%%1848" ,"Off"),
            ("%%1849" ,"Auto"),
        };

        public string ReplaceMessage(string mes)
        {
            foreach (var conv in ConvTable)
            {
                mes = mes.Replace(conv.From, conv.To);
            }
            return mes;
        }

        private IEnumerable<EventLogEntry> GetEventLog(EventLog log, CancellationToken cancellationToken)
        {
            if (log.Entries?.Count > 0)
            {
                var state = State.GetValueOrDefault(log.Log, true, a => new EventlogState
                {
                    LastIndex = -1,
                    LastUploadUtc = DateTime.UtcNow,
                });
                var ubound = log.Entries.Count - 1;
                var lastIndex = state.LastIndex;
                var fromIndex = log.Entries[ubound].Index;
                var expectedCount = fromIndex - lastIndex;

                for (var i = ubound; i >= 0 && expectedCount > 0; i--)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var e = log.Entries[i];
                    if (e.Index > lastIndex)
                    {
                        expectedCount--;
                        state.LastIndex = Math.Max(e.Index, state.LastIndex);
                        state.LastUploadUtc = DateTime.UtcNow;
                        yield return e;
                    }
                }
            }
        }

        private Dictionary<string, EventlogState> State = new Dictionary<string, EventlogState>();
        private string StateFileName => Path.Combine(StatePath, "EventlogListener.json");

        private void LoadState()
        {
            if (File.Exists(StateFileName) == false)
            {
                return;
            }
            try
            {
                var json = File.ReadAllText(StateFileName);
                State = JsonConvert.DeserializeObject<Dictionary<string, EventlogState>>(json);
            }
            catch
            {
            }
        }
        private void SaveState()
        {
            var json = JsonConvert.SerializeObject(State);
            File.WriteAllText(StateFileName, json);
        }
    }

    public class EventlogState
    {
        public long LastIndex { get; set; }
        public DateTime LastUploadUtc { get; set; }
    }

    public class EventlogMessageEventArgs : EventArgs
    {
        public DateTime EventUtcTime { get; set; }
        public string LogName { get; set; } // Security
        public string Source { get; set; }  // Microsoft Windows security auditing.
        public long EventID { get; set; }       //- 4624
        public string Computer { get; set; }    //- WINDESKTOP1234
        public string Message { get; set; }     //- An account was successfully logged on. ......
        public string User { get; set; }        //- N/A  |   SYSTEM
        public long EventRecordID { get; set; } //- 6547
        public string Level { get; set; }       //- Warning
        public string OpCode { get; set; }      //- Info
        public string TaskCategory { get; set; } //- None
        public string Keywords { get; set; }    //- 
    }
    public class EventlogErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
}
