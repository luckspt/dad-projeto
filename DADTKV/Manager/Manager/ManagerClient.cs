﻿using Common;
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
            try
            {
                this.GetClient(address).Crash(new CrashRequest());
            }
            catch (RpcException e)
            {
                if (e.InnerException.InnerException.Message.Contains("aborted"))
                    ; // ignore, intended behaviour
                else throw;
            }
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

        public void StartLeaseManager(string address, List<Peer> leaseManagersAddresses, List<Peer> transactionManagersAddresses, int proposerPosition)
        {
            this.GetClient(address).StartLeaseManager(new StartLeaseManagerRequest()
            {
                LeaseManagersAddresses = { leaseManagersAddresses.Select(x => x.FullRepresentation()) },
                TransactionManagersAddresses = { transactionManagersAddresses.Select(x => x.FullRepresentation()) },
                ProposerPosition = proposerPosition,
            });
        }

        public void StartTransactionManager(string address, List<Peer> leaseManagersAddresses, List<Peer> transactionManagersAddresses)
        {
            this.GetClient(address).StartTransactionManager(new StartTransactionManagerRequest()
            {
                LeaseManagersAddresses = { leaseManagersAddresses.Select(x => x.FullRepresentation()) },
                TransactionManagersAddresses = { transactionManagersAddresses.Select(x => x.FullRepresentation()) },
            });
        }

        private ManagerService.ManagerServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new ManagerService.ManagerServiceClient(serverChannel);
        }
    }
}
