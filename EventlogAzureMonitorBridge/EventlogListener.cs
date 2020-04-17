using Microsoft.Extensions.Logging;
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
        public Queue<EventlogMessage> Messages { get; set; }
        public ILogger<Worker> Logger { get; set; }

        public async Task<int> ListenAsync(CancellationToken stoppingToken)
        {
            await LoadStateAsync();

            var lotSize = 200;
            var waitTime = 5569;

            try
            {
                var logs = EventLog.GetEventLogs();
                var isLoop = true;
                var enqueueCount = 0;

                foreach (var log in logs.Where(a => isLoop))
                {
                    var recs = GetEventLog(log, stoppingToken).OrderBy(a => a.Index);   // order by requests the all collection query at the time.
                    lock (Messages)
                    {
                        foreach (var e in recs.Where(a => Messages.Count < lotSize))
                        {
                            stoppingToken.ThrowIfCancellationRequested();

                            var item = new EventlogMessage
                            {
                                EventUtcTime = e.TimeGenerated.ToUniversalTime(),
                                LogName = log.Log,
                                Source = e.Source,
                                EventID = e.InstanceId,
                                EventRecordID = e.Index,
                                User = e.UserName,
                                Computer = e.MachineName,
                                Message = EventlogMessage.ReplaceMessage(e.Message),
                            };
                            Messages.Enqueue(item);
                            enqueueCount++;
                            var cnt = Messages.Count;
                            waitTime = cnt >= lotSize ? 23 : 5569;
                            isLoop = cnt < lotSize;
                        }
                    }
                }
                if (enqueueCount > 0)
                {
                    await SaveStateAsync();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            return waitTime;
        }

        private Dictionary<string, EventlogState> State = new Dictionary<string, EventlogState>();
        private string StateFileName => Path.Combine(AppContext.BaseDirectory, "EventlogListener.json");

        public class EventlogState
        {
            public long LastIndex { get; set; }
            public DateTime LastUploadUtc { get; set; }
        }

        private async Task LoadStateAsync()
        {
            if (File.Exists(StateFileName) == false)
            {
                return;
            }
            try
            {
                var json = await File.ReadAllTextAsync(StateFileName);
                State = JsonConvert.DeserializeObject<Dictionary<string, EventlogState>>(json);
            }
            catch
            {
            }
        }

        private async Task SaveStateAsync()
        {
            var json = JsonConvert.SerializeObject(State);
            await File.WriteAllTextAsync(StateFileName, json);
        }

        private IEnumerable<EventLogEntry> GetEventLog(EventLog log, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
                    cancellationToken.ThrowIfCancellationRequested();

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
    }
}
