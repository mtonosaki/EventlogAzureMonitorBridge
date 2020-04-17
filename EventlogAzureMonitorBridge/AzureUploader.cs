using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventlogAzureMonitorBridge
{
    public class AzureUploader
    {
        public Queue<EventlogMessage> Messages { get; set; }
        public ILogger<Worker> Logger { get; set; }
        public string WorkspaceID { get; set; }
        public string PrimaryKey { get; set; }
        public string LogName { get; set; } = "Syslog";

        /// <summary>
        /// Upload thread
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task<int> UploadAsync(CancellationToken stoppingToken)
        {
            var lotsize = 200;
            var chunk = new List<LogRecord>();

            lock (Messages)
            {
                for (var i = 0; i < lotsize && Messages.Count > 0; i++)
                {
                    var ev = Messages.Dequeue();
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
                    chunk.Add(rec);
                }
            }
            if (chunk.Count == 0)
            {
                return 10567;
            }

            var jsonStr = JsonConvert.SerializeObject(chunk, new IsoDateTimeConverter());
            var datestring = DateTime.UtcNow.ToString("r");
            var jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
            var stringToHash = "POST\n" + jsonBytes.Length + "\napplication/json\n" + "x-ms-date:" + datestring + "\n/api/logs";
            var hashedString = BuildSignature(stringToHash, PrimaryKey);
            var signature = "SharedKey " + WorkspaceID + ":" + hashedString;
            //await PostDataAsync(signature, datestring, jsonStr);
#if DEBUG
            Logger.LogTrace($"---- Sent {chunk.Count} records at {DateTime.Now}", true);
#endif
            return chunk.Count == lotsize ? 23 : 5569;
        }

        private string BuildSignature(string message, string secret)
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

        public async Task PostDataAsync(string signature, string date, string json)
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
                var res = await client.SendAsync(req);
                //var responseContent = res.Content;
                //var result = await responseContent.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"API Post Exception: {ex.Message}");
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
