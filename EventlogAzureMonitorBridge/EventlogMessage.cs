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
    }
}
