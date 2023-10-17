using Common;
using Google.Protobuf.Collections;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Leases.LeaseUpdates
{
    public struct LeaseUpdate
    {
        public string Key;
        public List<string> TargetTMIds;
    }

    public struct LeaseUpdateRequest
    {
        public int Epoch;
        public List<LeaseUpdate> Leases;
    }

    internal class LeaseUpdateRequestDTO
    {
        static public LeaseUpdateRequest fromProtobuf(global::LeaseUpdateRequest updateRequest)
        {
            return new LeaseUpdateRequest
            {
                Epoch = updateRequest.Epoch,
                Leases = updateRequest.Leases.Select(lease => new LeaseUpdate
                {
                    Key = lease.Key,
                    TargetTMIds = lease.TargetTMIds.ToList()
                }).ToList()
            };
        }
    }

    internal class LeaseUpdatesService : global::LeaseUpdatesService.LeaseUpdatesServiceBase
    {
        private LeaseUpdatesServiceLogic serverLogic;

        public LeaseUpdatesService(LeaseUpdatesServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<LeaseUpdateResponse> LeaseUpdate(global::LeaseUpdateRequest request, ServerCallContext context)
        {
            return Task.FromResult(new LeaseUpdateResponse
            {
                Ok = this.serverLogic.LeaseUpdate(LeaseUpdateRequestDTO.fromProtobuf(request))
            });
        }
    }
}
