using Common;
using Google.Protobuf.Collections;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos.Server
{
    internal class TmLeasesDTO
    {
        static public LeaseStore fromProtobuf(RepeatedField<TmLeases> leases)
        {
            return LeaseStore.FromDict(leases.ToDictionary(lease => lease.Key, lease => lease.TmIds.ToList()));
        }

        static public RepeatedField<TmLeases> toProtobuf(LeaseStore leases)
        {
            RepeatedField<TmLeases> transactionLeases = new RepeatedField<TmLeases>();
            transactionLeases.AddRange(
                leases
                .Select(lease => new TmLeases
                {
                    Key = lease.Key,
                    TmIds = { lease.Value }
                }));

            return transactionLeases;
        }
    }

    internal class PaxosService : global::PaxosService.PaxosServiceBase
    {
        private PaxosServiceLogic serverLogic;

        public PaxosService(PaxosServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<global::PromiseResponse?> Prepare(global::PrepareRequest request, ServerCallContext context)
        {
            return Task.FromResult(PromiseResponseDTO.toProtobuf(serverLogic.Prepare(PrepareRequestDTO.fromProtobuf(request))));
        }

        public override Task<global::AcceptedResponse?> Accept(global::AcceptRequest request, ServerCallContext context)
        {
            return Task.FromResult(AcceptedResponseDTO.toProtobuf(serverLogic.Accept(AcceptRequestDTO.fromProtobuf(request))));
        }
    }
}
