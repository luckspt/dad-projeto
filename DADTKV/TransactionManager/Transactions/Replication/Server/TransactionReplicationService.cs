using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions.Replication.Server
{
    internal class TransactionReplicationService : global::TransactionReplicationService.TransactionReplicationServiceBase
    {
        private TransactionReplicationServiceLogic serverLogic;
        public TransactionReplicationService(TransactionReplicationServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<URBBroadcastResponse> URBBroadcast(URBBroadcastRequest request, ServerCallContext context)
        {
            return Task.FromResult(new URBBroadcastResponse
            {
                Ok = this.serverLogic.URBBroadcast(request.Message)
            });
        }

        public override Task<BEBDeliverResponse> BEBDeliver(BEBDeliverRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BEBDeliverResponse
            {
                Ok = this.serverLogic.BEBDeliver(request.Message, context.Peer)
            });
        }
    }
}
