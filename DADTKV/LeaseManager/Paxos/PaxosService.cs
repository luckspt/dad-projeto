using Google.Protobuf.Collections;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    internal class TmLeasesDTO
    {
        static public Dictionary<string, List<string>> fromProtobuf(RepeatedField<global::TmLeases> leases)
        {
            return leases.ToDictionary(lease => lease.Key, lease => lease.TmIds.ToList());
        }

        static public RepeatedField<global::TmLeases> toProtobuf(Dictionary<string, List<string>> leases)
        {
            RepeatedField<global::TmLeases> transactionLeases = new RepeatedField<global::TmLeases>();
            transactionLeases.AddRange(
                leases
                .Select(lease => new global::TmLeases
                {
                    Key = lease.Key,
                    TmIds = { lease.Value }
                }));

            return transactionLeases;
        }
    }

    internal class ClientService : PaxosService.PaxosServiceBase
    {
        private PaxosServiceLogic serverLogic;

        public ClientService(PaxosServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        #region Phase 1
        public override Task<PrepareResponse> Prepare(PrepareRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PrepareResponse { Ok = this.serverLogic.Prepare(request.Slot, request.Epoch, "TODO: HASH") });
        }

        public override Task<PromiseResponse> Promise(PromiseRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PromiseResponse { Ok = this.serverLogic.Promise(request.Slot, request.Epoch, TmLeasesDTO.fromProtobuf(request.Leases)) });
        }
        #endregion

        #region Phase 2
        public override Task<AcceptResponse> Accept(AcceptRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AcceptResponse { Ok = this.serverLogic.Accept(request.Slot, request.Epoch, TmLeasesDTO.fromProtobuf(request.Leases)) });
        }

        public override Task<AcceptedResponse> Accepted(AcceptedRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AcceptedResponse { Ok = this.serverLogic.Accepted(request.Slot, request.Epoch, TmLeasesDTO.fromProtobuf(request.Leases)) });
        }
        #endregion
    }
}
