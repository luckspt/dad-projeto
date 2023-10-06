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
        static public Dictionary<string, List<string>> fromProtobuf(RepeatedField<TmLeases> leases)
        {
            return leases.ToDictionary(lease => lease.Key, lease => lease.TmIds.ToList());
        }

        static public RepeatedField<TmLeases> toProtobuf(Dictionary<string, List<string>> leases)
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

    internal class PromiseResponseDTO
    {
        public static PromiseResponse? fromProtobuf(global::PromiseResponse? response)
        {
            if (response == null) return null;

            return new PromiseResponse
            {
                Slot = response.Slot,
                WriteEpoch = response.WriteEpoch,
                Leases = response.Leases == null ? null : TmLeasesDTO.fromProtobuf(response.Leases),
                SelfLeases = response.SelfLeases == null ? null : TmLeasesDTO.fromProtobuf(response.SelfLeases)
            };
        }

        public static global::PromiseResponse? toProtobuf(PromiseResponse? response)
        {
            if (response == null) return null;

            return new global::PromiseResponse
            {
                Slot = response.Value.Slot,
                WriteEpoch = response.Value.WriteEpoch,
                Leases = { response.Value.Leases == null ? null : TmLeasesDTO.toProtobuf(response.Value.Leases) },
                SelfLeases = { response.Value.SelfLeases == null ? null : TmLeasesDTO.toProtobuf(response.Value.SelfLeases) }
            };
        }
    }

    internal class AcceptedResponseDTO
    {
        public static AcceptedResponse? fromProtobuf(global::AcceptedResponse? response)
        {
            if (response == null) return null;

            return new AcceptedResponse
            {
                Slot = response.Slot,
                Epoch = response.Epoch,
                Leases = response.Leases == null ? null : TmLeasesDTO.fromProtobuf(response.Leases)
            };
        }

        public static global::AcceptedResponse? toProtobuf(AcceptedResponse? response)
        {
            if (response == null) return null;

            return new global::AcceptedResponse
            {
                Slot = response.Value.Slot,
                Epoch = response.Value.Epoch,
                Leases = { response.Value.Leases == null ? null : TmLeasesDTO.toProtobuf(response.Value.Leases) }
            };
        }
    }

    internal class PaxosService : global::PaxosService.PaxosServiceBase
    {
        private PaxosServiceLogic serverLogic;

        public PaxosService(PaxosServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<global::PromiseResponse?> Prepare(PrepareRequest request, ServerCallContext context)
        {

            return Task.FromResult(PromiseResponseDTO.toProtobuf(serverLogic.Prepare(request.Slot, request.Epoch, 0)));
        }

        public override Task<global::AcceptedResponse> Accept(AcceptRequest request, ServerCallContext context)
        {
            return Task.FromResult(AcceptedResponseDTO.toProtobuf(serverLogic.Accept(request.Slot, request.Epoch, TmLeasesDTO.fromProtobuf(request.Leases))));
        }
    }
}
