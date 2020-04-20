// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        Func<LinkedList<object>, string, bool>[] ReplaceSelector = new Func<LinkedList<object>, string, bool>[]
        {
            (build, num) => // MODE 0
            {
                if (NumberMessageTable.TryGetValue(int.Parse(num), out var str))
                {
                    build.AddFirst(str);
                    return true;
                }
                return false;
            },
            (build, num) => // MODE 1
            {
                if (SecurityIdentifiersTable.TryGetValue(num, out var str))
                {
                    build.AddFirst(str);
                    return true;
                }
                foreach( var kv in SecurityIdentifiersRegexTable)
                {
                    if( Regex.IsMatch(num, kv.Key))
                    {
                        build.AddFirst(kv.Value);
                        return true;
                    }
                }
                return false;
            },
        };

        /// <summary>
        /// Convert Message
        /// </summary>
        /// <param name="mes"></param>
        /// <returns></returns>
        public string Convert(string mes)
        {
            var pre = new[] { "%%", "%{" };
            var build = new LinkedList<object>();
            var len = mes.Length;
            var st = len;
            var ed = len;
            for (var limit = 5000; limit > 0; limit--)
            {
                Func<int, bool> LoopCondition;
                int mode;
                var isError = false;
                var pp1 = mes.LastIndexOf(pre[0], st);
                var pp2 = mes.LastIndexOf(pre[1], st);
                var pp = Math.Max(pp1, pp2);
                var post = "";
                if (pp1 > pp2)
                {
                    mode = 0;
                    LoopCondition = i => i < ed && char.IsDigit(mes[i]);
                }
                else
                {
                    mode = 1;
                    LoopCondition = i => i < ed && mes[i] != '}' && mes[i] != '\r' && mes[i] != '\n';
                }
                if (pp >= 0)
                {
                    var num = "";
                    int i;
                    for (i = pp + 2; LoopCondition(i); i++)
                    {
                        num += mes[i];
                    }
                    if (mode == 1 && i < len)
                    {
                        if (mes[i] == '}')
                        {
                            i++;
                            post = "}";
                        }
                        else
                        {
                            isError = true;
                        }
                    }
                    if (i < ed)
                    {
                        build.AddFirst((i, ed - i));    // after NUMBER
                        ed = i;
                    }
                    if (num != "")
                    {
                        if (isError || ReplaceSelector[mode](build, num) == false)
                        {
                            build.AddFirst($"{pre[mode]}{num}{post}");
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
