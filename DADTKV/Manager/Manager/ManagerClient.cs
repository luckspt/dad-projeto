using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.Manager
{
    public class ManagerClient
    {
        public void Crash(string address)
        {
            this.GetClient(address).Crash(new CrashRequest());
        }

        public void CommunicationDelay(string address, int delayMs)
        {
            this.GetClient(address).CommunicationDelay(new CommunicationDelayRequest()
            {
                DelayMsPerRequest = delayMs,
            });
        }

        public void StatusHookConfig(string address, bool enabled, int hookIntervalMs)
        {
            this.GetClient(address).StatusHookConfig(new StatusHookConfigRequest()
            {
                Enabled = enabled,
                HookIntervalMs = hookIntervalMs,
            });
        }

        private ManagerService.ManagerServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new ManagerService.ManagerServiceClient(serverChannel);
        }
    }
}
