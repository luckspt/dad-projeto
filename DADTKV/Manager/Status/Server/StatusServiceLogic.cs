using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace Manager.Status.Server
{
    internal class StatusServiceLogic
    {
        private List<Peer> peers;
        public StatusServiceLogic(List<Peer> peers)
        {
            this.peers = peers;
        }

        public void Status()
        {
            foreach (Peer peer in peers)
            {
                try
                {
                    Logger.GetInstance().Log("StatusService", $"Asking {peer.Id} to display its status");
                    this.GetClient(peer.Address).StatusAsync(new Empty());
                }
                catch { }
            }
        }

        private global::StatusService.StatusServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new global::StatusService.StatusServiceClient(serverChannel);
        }
    }
}
