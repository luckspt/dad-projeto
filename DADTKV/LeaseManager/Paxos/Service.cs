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

    internal class ClientService : PaxosService.PaxosServiceClient
    {
        private Phase1 phase1;
        private Phase2 phase2;

        public ClientService(Phase1 phase1, Phase2 phase2)
        {
            this.phase1 = phase1;
            this.phase2 = phase2;
        }

        // TODO: do the functions return have to be a Task?

        #region Phase 1
        public override PrepareResponse Prepare(PrepareRequest request, CallOptions options)
        {
            return new PrepareResponse { Ok = this.phase1.Prepare(request.Epoch) };
        }

        public override PromiseResponse Promise(PromiseRequest request, CallOptions options)
        {
            return new PromiseResponse { Ok = this.phase1.Promise(request.Epoch, TmLeasesDTO.fromProtobuf(request.Leases)) };
        }
        #endregion

        #region Phase 2
        public override AcceptResponse Accept(AcceptRequest request, CallOptions options)
        {
            return new AcceptResponse { Ok = this.phase2.Accept(request.Epoch, TmLeasesDTO.fromProtobuf(request.Leases)) };
        }

        public override AcceptedResponse Accepted(AcceptedRequest request, CallOptions options)
        {
            return new AcceptedResponse { Ok = this.phase2.Accepted(request.Epoch, TmLeasesDTO.fromProtobuf(request.Leases)) };
        }
        #endregion

    }
}
