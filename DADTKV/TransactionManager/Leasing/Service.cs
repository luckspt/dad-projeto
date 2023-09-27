using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    internal class LeaseDTO
    {
        static public List<Lease> fromProtobuf(TransactionLeases leases)
        {
            return leases.Leases
                 .Select(lease => new Lease(lease.Key, lease.Epoch, lease.TargetTMId))
                 .ToList();
        }

        static public TransactionLeases toProtobuf(List<Lease> leases)
        {
            TransactionLeases transactionLeases = new TransactionLeases();
            transactionLeases.Leases.AddRange(leases
                .Select(lease => new global::Lease {
                    Key = lease.Key,
                    Epoch = lease.Epoch,
                    TargetTMId = lease.TargetTMId }));

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

        #region Phase 1
        public override PrepareResponse Prepare(PrepareRequest request, CallOptions options)
        {
            return new PrepareResponse { Ok = this.phase1.Prepare(request.Epoch) };
        }

        public override PromiseResponse Promise(PromiseRequest request, CallOptions options)
        {
            return new PromiseResponse { Ok = this.phase1.Promise(request.Epoch, LeaseDTO.fromProtobuf(request.Leases)) };
        }
        #endregion

        #region Phase 2
        public override AcceptResponse Accept(AcceptRequest request, CallOptions options)
        {
            return new AcceptResponse { Ok = this.phase2.Accept(request.Epoch, LeaseDTO.fromProtobuf(request.Leases)) };
        }

        public override AcceptedResponse Accepted(AcceptedRequest request, CallOptions options)
        {
            return new AcceptedResponse { Ok = this.phase2.Accepted(request.Epoch, LeaseDTO.fromProtobuf(request.Leases)) };
        }
        #endregion

    }
}
