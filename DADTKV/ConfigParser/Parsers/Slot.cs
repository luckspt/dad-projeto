﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public struct SlotsConfigLine : ConfigLine
    {
        public int Count;
    }

    internal class SlotsParser : Parser
    {
        private Regex regex = new Regex(@"^S (\d+) *$", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.Slots,
                new SlotsConfigLine
                {
                    Count = int.Parse(match.Groups[1].Value),
                }
            );
        }
    }
}
