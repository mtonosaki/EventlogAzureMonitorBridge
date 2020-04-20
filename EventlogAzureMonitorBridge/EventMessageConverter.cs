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
            var build = new LinkedList<object>();
            var len = mes.Length;
            var st = len;
            var ed = len;
            for (var limit = 5000; limit > 0 ; limit-- )
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
                        build.AddFirst((i, ed - i));    // after NUMBER
                        ed = i;
                    }
                    if (num != "")
                    {
                        if (Table.TryGetValue(int.Parse(num), out var str))
                        {
                            build.AddFirst(str);
                        }
                        else
                        {
                            build.AddFirst($"%%{num}");
                        }
                        ed = pp;
                    }
                    st = pp;
                }
                else
                {
                    build.AddFirst((0, ed));
                    break;
                }
            }
            var ret = new StringBuilder();
            foreach (var node in build)
            {
                if (node is ValueTuple<int, int> ii)
                {
                    ret.Append(mes.Substring(ii.Item1, ii.Item2));
                }
                else
                if (node is string str)
                {
                    ret.Append(str);
                }
            }
            return ret.ToString();
        }
    }
}
