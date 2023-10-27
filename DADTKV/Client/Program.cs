using Common;
using Grpc.Core;
using Parser;
using Parser.Parsers.ClientCommand;

namespace Client
{
    internal static class Program
    {
        /// <summary>
        /// Application entrypoint
        /// </summary>
        /// <param name="args">string[] { managerAddress, entityId, scriptPath, selectedTransactionManager ...transactionManagersAddresses}</param>
        static void Main(string[] args)
        {
            Logger.GetInstance().Log("Client", "Starting Client...");
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            ConfigParser configParser = new ConfigParser(args[2]);
            configParser.Parse();

            List<ClientCommandConfigLine> commands = configParser.Config[ConfigType.ClientCommand]
                                                .ConvertAll(line => (ClientCommandConfigLine)line);

            List<Peer> transactionManagers = args[4..]
                                                .Select(raw => Peer.FromString(raw))
                                                .ToList();

            Client client = new Client(args[1], args[0], int.Parse(args[3]), commands, transactionManagers);
            client.Start();
        }
    }
}
