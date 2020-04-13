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
            while (cancellationToken.IsCancellationRequested == false)
            {
                await Task.Delay(10567, cancellationToken); // upload 10s each to reduce request count to Azure.

                var queue = Messages?.Invoke();
                List<EventlogMessageEventArgs> chunk;

                lock (queue)
                {
                    chunk = queue.ToList();
                    queue.Clear();
                }
                if (chunk.Count < 1)
                {
                    continue;
                }
                var recs = new List<LogRecord>();
                foreach (var ev in chunk)
                {
                    var rec = new LogRecord
                    {
                        Facility = $"Eventlog.{ev.LogName}",
                        SeverityLevel = ev.Level,
                        EventTime = ev.EventUtcTime,
                        Computer = ev.Computer,
                        SyslogMessage = ev.Message,
                        HostIP = null,
                        HostName = "(n/a)",
                    };
                    recs.Add(rec);
                }
                var jsonStr = JsonConvert.SerializeObject(recs, new IsoDateTimeConverter());
                var datestring = DateTime.UtcNow.ToString("r");
                var jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
                var stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
                var hashedString = BuildSignature(stringToHash, Key1);
                var signature = "SharedKey " + WorkspaceID + ":" + hashedString;
                PostData(signature, datestring, jsonStr);
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
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Log-Type", LogName);
                request.Headers.Add("Authorization", signature);
                request.Headers.Add("x-ms-date", date);
                request.Headers.Add("time-generated-field", "");
                var httpContent = new StringContent(json, Encoding.UTF8);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Content = httpContent;
                var response = client.SendAsync(request);
                var responseContent = response.Result.Content;  // wait the responce of request
                string result = responseContent.ReadAsStringAsync().Result;
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
