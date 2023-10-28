using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class StatusServiceClient
    {
        private StatusService.StatusServiceClient client;
        public StatusServiceClient(string managerAddress)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(managerAddress);
            this.client = new StatusService.StatusServiceClient(serverChannel);
        }

        public void Status()
        {
            this.client.Status(new Empty());
        }
    }
}
