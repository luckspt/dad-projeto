using Grpc.Core;
using Grpc.Net.Client;
using ManagerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerClientServices
{
    public class ManagerClient
    {
        private ManagerStatusHook.ManagerStatusHookClient client;
        private string entityId;
        private EntityType entityType;
        private string status;
        public string Status
        {
            get => this.status;
            set
            {
                lock (this.status)
                {
                    this.status = value;
                }
            }
        }

        public ManagerClient(string entityId, EntityType entityType, string hostname = "localhost", int port = 9999, bool insecure = true)
        {
            this.entityId = entityId;
            this.entityType = entityType;
            this.status = "NotStarted";

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", insecure);
            string protocol = insecure ? "http" : "https";

            GrpcChannel serverChannel = GrpcChannel.ForAddress($"{protocol}://{hostname}:{port}");
            this.client = new ManagerStatusHook.ManagerStatusHookClient(serverChannel);
        }

        public void ExecuteHook(object state)
        {
            this.client.ExecuteAsync(new ExecuteRequest()
            {
                Id = this.entityId,
                Type = this.entityType,
                Status = this.Status,
            });
        }
    }
}
