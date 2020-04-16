// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tono;

namespace EventlogAzureMonitorBridge
{
    public class AzureUploader
    {
        public string WorkspaceID { get; set; }
        public string Key1 { get; set; }
        public string LogName { get; set; } = "Syslog";
        public Action<string, bool> Logger { get; set; }

        public Func<Queue<EventlogMessageEventArgs>> Messages { get; set; }

        public async Task PorlingMessagesAsync(CancellationToken cancellationToken)
        {
            var lotsize = 200;
            var waitTime = 5569; // 23, 3343, 5569, 10567　... prime numbers
            while (cancellationToken.IsCancellationRequested == false)
            {
                await Task.Delay(waitTime, cancellationToken); // upload 10s each to reduce request count to Azure.

                var recs = new List<LogRecord>();
                var queue = Messages?.Invoke();
                lock (queue)
                {
                    for (var i = 0; i < lotsize && queue.Count > 0; i++)
                    {
                        var ev = queue.Dequeue();
                        var rec = new LogRecord
                        {
                            EventTime = ev.EventUtcTime,
                            Facility = $"Eventlog.{ev.LogName}",
                            SeverityLevel = ev.Level,
                            Computer = ev.Computer,
                            HostIP = null,
                            HostName = null,
                            SyslogMessage = ev.Message,
                            RecordID = ev.EventRecordID,
                            EventID = ev.EventID,
                            User = ev.User,
                            OpCode = ev.OpCode,
                            TaskCategory = ev.TaskCategory,
                            Keywords = ev.Keywords,
                        };
                        recs.Add(rec);
                    }
                }
                if (recs.Count == 0)
                {
                    waitTime = 10567;
                    continue;
                }
                else if (recs.Count == lotsize)
                {
                    waitTime = 23;
                }
                else
                {
                    waitTime = 3343;
                }
                var jsonStr = JsonConvert.SerializeObject(recs, new IsoDateTimeConverter());
                var datestring = DateTime.UtcNow.ToString("r");
                var jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
                var stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
                var hashedString = BuildSignature(stringToHash, Key1);
                var signature = "SharedKey " + WorkspaceID + ":" + hashedString;
                PostData(signature, datestring, jsonStr);
                Logger?.Invoke($"---- Sent {recs.Count} records at {DateTime.Now}", true);
            }
        }

        public string BuildSignature(string message, string secret)
        {
            var encoding = new ASCIIEncoding();
            var keyByte = Convert.FromBase64String(secret);
            var messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var hash = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }

        private static readonly HttpClient client = new HttpClient();

        public void PostData(string signature, string date, string json)
        {
            try
            {
                var url = "https://" + WorkspaceID + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";
                var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Add("Accept", "application/json");
                req.Headers.Add("Log-Type", LogName);
                req.Headers.Add("Authorization", signature);
                req.Headers.Add("x-ms-date", date);
                req.Headers.Add("time-generated-field", "");
                var httpContent = new StringContent(json, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                req.Content = httpContent;
                var res = client.SendAsync(req);
                var responseContent = res.Result.Content;  // wait the responce of request
                //var result = responseContent.ReadAsStringAsync().Result;
            }
            catch (Exception excep)
            {
                Logger?.Invoke("API Post Exception: " + excep.Message, true);
            }
        }

        public class LogRecord
        {
            public DateTime EventTime { get; set; }     // 2020-01-21T22:33:33Z
            public string Facility { get; set; }        // $"eventlog.{obj.LogName}"
            public string SeverityLevel { get; set; }   // obj.Level
            public string Computer { get; set; }        // obj.Computer
            public string HostIP { get; set; }          // (null)
            public string HostName { get; set; }        // (null)
            public string SyslogMessage { get; set; }   // obj.Message

            //--- EventLog Original ---
            public long RecordID { get; set; }          // obj.EventRecordID
            public long EventID { get; set; }           // obj.EventID
            public string User { get; set; }            // obj.User
            public string OpCode { get; set; }          // Info
            public string TaskCategory { get; set; }    // None
            public string Keywords { get; set; }        // 
        };
    }
}
