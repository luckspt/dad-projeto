using Common;
using Grpc.Core;
using Grpc.Net.Client;
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

        public ManagerClient(HostPort managerAddress, string entityId, EntityType entityType)
        {
            this.entityId = entityId;
            this.entityType = entityType;
            this.status = "Idle";

            GrpcChannel serverChannel = GrpcChannel.ForAddress(managerAddress.ToString());
            this.client = new ManagerStatusHook.ManagerStatusHookClient(serverChannel);
        }

        public void ExecuteHook(object state)
        {
            this.client.Execute(new ExecuteRequest()
            {
                Id = this.entityId,
                Type = this.entityType,
                Status = this.Status,
            });
        }
    }
}
