using Common;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions.Replication.Client
{
    internal class TransactionReplicationServiceClient
    {
        private TransactionReplication transactionReplication;
        public TransactionReplicationServiceClient(TransactionReplication transactionReplication)
        {
            this.transactionReplication = transactionReplication;
        }

        public void URBroadcast(BroadcastMessage message)
        {
            lock (this.transactionReplication.Correct)
            {
                foreach (Peer correct in this.transactionReplication.Correct)
                {
                    // TODO do we need to track messages or can we just send and forget?
                    this.GetClient(correct.Address).URBBroadcastAsync(new URBBroadcastRequest
                    {
                        Message = message
                    });
                }
            }
        }

        public void BEBroadcast(BroadcastMessage message)
        {
            lock (this.transactionReplication.Correct)
            {
                foreach (Peer correct in this.transactionReplication.Correct)
                {
                    // TODO do we need to track messages or can we just send and forget?
                    this.GetClient(correct.Address).BEBDeliverAsync(new BEBDeliverRequest
                    {
                        Message = message
                    });
                }
            }
        }

        private global::TransactionReplicationService.TransactionReplicationServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new global::TransactionReplicationService.TransactionReplicationServiceClient(serverChannel);
        }
    }
}
