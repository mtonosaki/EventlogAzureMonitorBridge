// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace EventlogAzureMonitorBridge
{
    public partial class AzureUploader
    {
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
