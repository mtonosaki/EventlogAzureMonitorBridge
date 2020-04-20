// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace EventlogAzureMonitorBridge
{
    /// <summary>
    /// Eventlog Message Table
    /// </summary>
    public partial class EventMessageConverter
    {
        public static string ConvertFrom(string mes)
        {
            var emc = new EventMessageConverter();
            return emc.Convert(mes);
        }

        /// <summary>
        /// Convert Message
        /// </summary>
        /// <param name="mes"></param>
        /// <returns></returns>
        public string Convert(string mes)
        {
            var ret = new StringBuilder();
            var len = mes.Length;
            var st = len;
            var ed = len;
            for (; ; )
            {
                var pp = mes.LastIndexOf("%%", st);
                if (pp >= 0)
                {
                    var num = "";
                    int i;
                    for (i = pp + 2; i < ed && char.IsDigit(mes[i]); i++)
                    {
                        num += mes[i];
                    }
                    if (i < ed)
                    {
                        ret.Insert(0, mes.Substring(i, ed - i));    // after NUMBER
                        ed = i;
                    }
                    if (num != "")
                    {
                        if (Table.TryGetValue(int.Parse(num), out var str))
                        {
                            ret.Insert(0, str);
                        }
                        else
                        {
                            ret.Insert(0, $"%%{num}");
                        }
                        ed = pp;
                    }
                    st = pp;
                }
                else
                {
                    ret.Insert(0, mes.Substring(0, ed));
                    break;
                }
            }
            return ret.ToString();
        }
    }
}
