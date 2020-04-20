﻿// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;

namespace EventlogAzureMonitorBridge
{
    public partial class EventMessageConverter
    {
        /// <summary>
        /// Well-known security identifiers in Windows operating systems
        /// </summary>
        /// <seealso cref="https://support.microsoft.com/en-us/help/243330/well-known-security-identifiers-in-windows-operating-systems"/>
        static readonly IReadOnlyDictionary<string, string> SecurityIdentifiersTable = new Dictionary<string, string>
        {
            [@"S-1-0"] = @"Null Authority",
            [@"S-1-0-0"] = @"Nobody",
            [@"S-1-1"] = @"World Authority",
            [@"S-1-1-0"] = @"Everyone",
            [@"S-1-16-0"] = @"Untrusted Mandatory Level",
            [@"S-1-16-12288"] = @"High Mandatory Level",
            [@"S-1-16-16384"] = @"System Mandatory Level",
            [@"S-1-16-20480"] = @"Protected Process Mandatory Level",
            [@"S-1-16-28672"] = @"Secure Process Mandatory Level",
            [@"S-1-16-4096"] = @"Low Mandatory Level",
            [@"S-1-16-8192"] = @"Medium Mandatory Level",
            [@"S-1-16-8448"] = @"Medium Plus Mandatory Level",
            [@"S-1-2"] = @"Local Authority",
            [@"S-1-2-0"] = @"Local",
            [@"S-1-2-1"] = @"Console Logon",
            [@"S-1-3"] = @"Creator Authority",
            [@"S-1-3-0"] = @"Creator Owner",
            [@"S-1-3-1"] = @"Creator Group",
            [@"S-1-3-2"] = @"Creator Owner Server",
            [@"S-1-3-3"] = @"Creator Group Server",
            [@"S-1-3-4"] = @"Owner Rights",
            [@"S-1-4"] = @"Non-unique Authority",
            [@"S-1-5"] = @"NT Authority",
            [@"S-1-5-1"] = @"Dialup",
            [@"S-1-5-10"] = @"Principal Self",
            [@"S-1-5-11"] = @"Authenticated Users",
            [@"S-1-5-12"] = @"Restricted Code",
            [@"S-1-5-13"] = @"Terminal Server Users",
            [@"S-1-5-14"] = @"Remote Interactive Logon",
            [@"S-1-5-15"] = @"This Organization",
            [@"S-1-5-17"] = @"This Organization",
            [@"S-1-5-18"] = @"Local System",
            [@"S-1-5-19"] = @"NT Authority",
            [@"S-1-5-2"] = @"Network",
            [@"S-1-5-20"] = @"NT Authority",
            [@"S-1-5-3"] = @"Batch",
            [@"S-1-5-32-544"] = @"Administrators",
            [@"S-1-5-32-545"] = @"Users",
            [@"S-1-5-32-546"] = @"Guests",
            [@"S-1-5-32-547"] = @"Power Users",
            [@"S-1-5-32-548"] = @"Account Operators",
            [@"S-1-5-32-549"] = @"Server Operators",
            [@"S-1-5-32-550"] = @"Print Operators",
            [@"S-1-5-32-551"] = @"Backup Operators",
            [@"S-1-5-32-552"] = @"Replicators",
            [@"S-1-5-32-554"] = @"Builtin\Pre-Windows 2000 Compatible Access",
            [@"S-1-5-32-555"] = @"Builtin\Remote Desktop Users",
            [@"S-1-5-32-556"] = @"Builtin\Network Configuration Operators",
            [@"S-1-5-32-557"] = @"Builtin\Incoming Forest Trust Builders",
            [@"S-1-5-32-558"] = @"Builtin\Performance Monitor Users",
            [@"S-1-5-32-559"] = @"Builtin\Performance Log Users",
            [@"S-1-5-32-560"] = @"Builtin\Windows Authorization Access Group",
            [@"S-1-5-32-561"] = @"Builtin\Terminal Server License Servers",
            [@"S-1-5-32-562"] = @"Builtin\Distributed COM Users",
            [@"S-1-5-32-569"] = @"Builtin\Cryptographic Operators",
            [@"S-1-5-32-573"] = @"Builtin\Event Log Readers",
            [@"S-1-5-32-574"] = @"Builtin\Certificate Service DCOM Access",
            [@"S-1-5-32-575"] = @"Builtin\RDS Remote Access Servers",
            [@"S-1-5-32-576"] = @"Builtin\RDS Endpoint Servers",
            [@"S-1-5-32-577"] = @"Builtin\RDS Management Servers",
            [@"S-1-5-32-578"] = @"Builtin\Hyper-V Administrators",
            [@"S-1-5-32-579"] = @"Builtin\Access Control Assistance Operators",
            [@"S-1-5-32-580"] = @"Builtin\Remote Management Users",
            [@"S-1-5-32-582"] = @"Storage Replica Administrators",
            [@"S-1-5-4"] = @"Interactive",
            [@"S-1-5-6"] = @"Service",
            [@"S-1-5-64-10"] = @"NTLM Authentication",
            [@"S-1-5-64-14"] = @"SChannel Authentication",
            [@"S-1-5-64-21"] = @"Digest Authentication",
            [@"S-1-5-64-36"] = @"Cloud Account Authentication",
            [@"S-1-5-7"] = @"Anonymous",
            [@"S-1-5-8"] = @"Proxy",
            [@"S-1-5-80"] = @"NT Service",
            [@"S-1-5-80-0"] = @"All Services",
            [@"S-1-5-80-0"] = @"NT Services\All Services",
            [@"S-1-5-83-0"] = @"NT Virtual Machine\Virtual Machines",
            [@"S-1-5-9"] = @"Enterprise Domain Controllers",
            [@"S-1-5-90-0"] = @"Windows Manager\Windows Manager Group",
        };

        /// <summary>
        /// Well-known security identifiers in Windows operating systems
        /// </summary>
        /// <seealso cref="https://support.microsoft.com/en-us/help/243330/well-known-security-identifiers-in-windows-operating-systems"/>
        static readonly IReadOnlyList<(string Key, string Value)> SecurityIdentifiersRegexTable = new List<(string Key, string Value)>
        {
            (@"S-1-5-5-[0-9]+-[0-9]+",   @"Logon Session"),
            (@"S-1-5-21-[0-9]+-498", @"Enterprise Read-only Domain Controllers"),
            (@"S-1-5-21-[0-9]+-500", @"Administrator"),
            (@"S-1-5-21-[0-9]+-501", @"Guest"),
            (@"S-1-5-21-[0-9]+-502", @"KRBTGT"),
            (@"S-1-5-21-[0-9]+-512", @"Domain Admins"),
            (@"S-1-5-21-[0-9]+-513", @"Domain Users"),
            (@"S-1-5-21-[0-9]+-514", @"Domain Guests"),
            (@"S-1-5-21-[0-9]+-515", @"Domain Computers"),
            (@"S-1-5-21-[0-9]+-516", @"Domain Controllers"),
            (@"S-1-5-21-[0-9]+-517", @"Cert Publishers"),
            (@"S-1-5-21-[0-9]+-520", @"Group Policy Creator Owners"),
            (@"S-1-5-21-[0-9]+-521", @"Read-only Domain Controllers"),
            (@"S-1-5-21-[0-9]+-522", @"Cloneable Domain Controllers"),
            (@"S-1-5-21-[0-9]+-526", @"Key Admins"),
            (@"S-1-5-21-[0-9]+-527", @"Enterprise Key Admins"),
            (@"S-1-5-21-[0-9]+-553", @"RAS and IAS Servers"),
            (@"S-1-5-21-[0-9]+-571", @"Allowed RODC Password Replication Group"),
            (@"S-1-5-21-[0-9]+-572", @"Denied RODC Password Replication Group"),
            (@"S-1-5-21-[0-9]+-518", @"Schema Admins"),
            (@"S-1-5-21-[0-9]+-519", @"Enterprise Admins"),
        };
    }
}
