using Common;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using LeaseManager.Paxos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Leasing.Updating
{
    public struct LeaseUpdateRequest
    {
        public int Epoch;
        public Dictionary<string, List<string>> Leases;
    }

    internal class LeaseUpdateRequestDTO
    {
        static public global::LeaseUpdateRequest toProtobuf(LeaseUpdateRequest request)
        {
            return new global::LeaseUpdateRequest
            {
                Epoch = request.Epoch,
                Leases = { request.Leases.Select(x => new LeaseUpdate
                {
                    Key = x.Key,
                    TargetTMIds = { x.Value.ToList() }
                }).ToList() }
            };
        }
    }

    internal class LeaseUpdatesServiceClient
    {
        private PaxosInstance paxosInstance;
        public LeaseUpdatesServiceClient(PaxosInstance paxosInstance)
        {
            this.paxosInstance = paxosInstance;
        }

        public void LeaseUpdate(LeaseUpdateRequest update)
        {
            Logger.GetInstance().Log($"LeaseUpdatesServiceClient.{this.paxosInstance.Slot}", $"Sending updates to learners");
            foreach (Peer learner in this.paxosInstance.Learners)
            {
                new Task(() =>
                {
                    try
                    {
                        this.GetClient(learner.Address).LeaseUpdate(LeaseUpdateRequestDTO.toProtobuf(update));
                    }
                    catch (RpcException e)
                    {
                        Logger.GetInstance().Log("LeaseUpdate", e.Message);
                    }
                }).Start();
            }
        }

        private LeaseUpdatesService.LeaseUpdatesServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new LeaseUpdatesService.LeaseUpdatesServiceClient(serverChannel);
        }
    }
}
