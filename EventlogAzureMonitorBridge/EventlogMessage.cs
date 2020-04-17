// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;

namespace EventlogAzureMonitorBridge
{
    public class EventlogMessage
    {
        public DateTime EventUtcTime { get; set; }
        public string LogName { get; set; }     // Security
        public string Source { get; set; }      // Microsoft Windows security auditing.
        public long EventID { get; set; }       //- 4624
        public string Computer { get; set; }    //- WINDESKTOP1234
        public string Message { get; set; }     //- An account was successfully logged on. ......
        public string User { get; set; }        //- N/A  |   SYSTEM
        public long EventRecordID { get; set; } //- 6547
        public string Level { get; set; }       //- Warning
        public string OpCode { get; set; }      //- Info
        public string TaskCategory { get; set; } //- None
        public string Keywords { get; set; }    //- 

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

        public static string ReplaceMessage(string mes)
        {
            foreach (var conv in ConvTable)
            {
                mes = mes.Replace(conv.From, conv.To);
            }
            return mes;
        }
    }
}
