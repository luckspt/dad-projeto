﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser.Parsers
{
    public struct ClientConfigLine : ConfigLine
    {
        public string ID;
        public string ScriptPath;
    }

    internal class ClientParser : Parser
    {
        private Regex regex = new Regex(@"^P (\w+) C (.+) *$", RegexOptions.Compiled);
        public Tuple<ConfigType, ConfigLine>? Result(string line)
        {
            Match match = this.regex.Match(line);
            if (!match.Success) return null;

            return new Tuple<ConfigType, ConfigLine>(
                ConfigType.Client,
                new ClientConfigLine
                {
                    ID = match.Groups[1].Value,
                    ScriptPath= match.Groups[2].Value
                }
            );
        }
    }
}
