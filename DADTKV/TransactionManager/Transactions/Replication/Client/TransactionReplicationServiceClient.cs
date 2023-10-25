using Common;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TransactionManager.Leases.LeaseUpdates;
using TransactionManager.Transactions.Replication.Server;

namespace TransactionManager.Transactions.Replication.Client
{
    internal class TransactionReplicationServiceClient
    {
        private TransactionReplication transactionReplication;
        public TransactionReplicationServiceClient(TransactionReplication transactionReplication)
        {
            this.transactionReplication = transactionReplication;
        }

        public void URBroadcast(BroadcastMessage message, string senderId)
        {
            lock (this.transactionReplication.Correct)
            {

                foreach (Peer correct in this.transactionReplication.Correct)
                {
                    new Task(() =>
                    {
                        try
                        {
                            this.GetClient(correct.Address).URBBroadcast(new URBBroadcastRequest
                            {
                                Message = BroadcastMessageDTO.toProtobuf(message),
                                SenderId = senderId
                            });
                            Logger.GetInstance().Log($"TransactionReplicationService.URB", $"Sending message to {correct.Address}");
                        }
                        catch (RpcException e)
                        {
                            Logger.GetInstance().Log("URBroadcast", e.Message);
                        }
                    }).Start();
                }
            }
        }

        public void BEBroadcast(BroadcastMessage message, string senderId)
        {
            lock (this.transactionReplication.Correct)
            {
                foreach (Peer correct in this.transactionReplication.Correct)
                {
                    new Task(() =>
                    {
                        try
                        {
                            Logger.GetInstance().Log($"TransactionReplicationService.BEB", $"Sending message to {correct.Address}");
                            this.GetClient(correct.Address).BEBDeliver(new BEBDeliverRequest
                            {
                                Message = BroadcastMessageDTO.toProtobuf(message),
                                SenderId = senderId
                            });
                        }
                        catch (RpcException e)
                        {
                            Logger.GetInstance().Log("BEBroadcast", e.Message);
                        }
                    }).Start();
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
