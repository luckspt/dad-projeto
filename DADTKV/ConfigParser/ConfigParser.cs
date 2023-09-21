﻿using Parser.Parsers;

namespace Parser
{
    public enum ConfigType
    {
        // Server Identification (P)
        Server,
        // Client Identification (C)
        Client,
        // Running time (S)
        Time,
        // Start time (T)
        StartTime,
        // Time slot (D)
        TimeSlot,
        // Server State (F)
        ServerState
    }

    public interface ConfigLine {
    }

    public interface Parser
    {
        Tuple<ConfigType, ConfigLine>? Result(string line);
    }

    public class ConfigParser
    {
        private string Path { get; }
        private string CommentChars { get; }
        private List<Parser> parsers;

        private Dictionary<ConfigType, List<ConfigLine>> Config { get; }

        public ConfigParser(string path) : this(path, new List<Parser>() { new ClientParser(), new ServerParser(), new ServerStateParser(), new StartTimeParser(), new TimeParser(), new TimeSlotParser()}, "#")
        {}

        public ConfigParser(string path, List<Parser> parsers) : this(path, parsers, "#")
        {}

        public ConfigParser(string path, List<Parser> parsers, string commentChars)
        {
            this.Path = path;
            this.parsers = parsers;
            this.CommentChars = commentChars;
            this.Config = new Dictionary<ConfigType, List<ConfigLine>>();
        }

        public void Parse()
        {
            using (StreamReader sr = new StreamReader(this.Path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // Ignore comments
                    if (line.StartsWith(this.CommentChars)) continue;

                    // Parse line
                    try
                    {
                        var configLine = this.parsers
                            .Where(p => p.Result(line) != null)
                            .Select(p => p.Result(line))
                            .First();

                        if (configLine == null) continue;

                        if (this.Config.ContainsKey(configLine.Item1))
                            this.Config[configLine.Item1].Add(configLine.Item2);
                        else
                            this.Config[configLine.Item1] = new List<ConfigLine> { configLine.Item2 };
                    }
                    catch (Exception ex)
                    {
                        if (ex is ArgumentNullException || ex is InvalidOperationException)
                        {
                            Console.WriteLine("No parser found for line: " + line);
                        }

                        throw;
                    }
                }
            }
        }
    }
}