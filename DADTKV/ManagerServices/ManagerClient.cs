using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerClientServices
{
    public struct HostPort
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class ManagerClient
    {
        private GrpcChannel serverChannel = null;
        public ManagerClient(HostPort client, HostPort server, bool insecure = true)
        {
            string protocol = insecure ? "http" : "https";
            this.serverChannel = GrpcChannel.ForAddress($"{protocol}://{server.Host}:{server.Port}");
        }
    }
}
