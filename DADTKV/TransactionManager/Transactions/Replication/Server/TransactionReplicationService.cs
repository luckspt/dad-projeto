using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions.Replication.Server
{
    internal class RPCStoreDadIntDTO
    {
        public static RPCStoreDadInt fromProtobuf(global::RPCStoreDadInt dadInt)
        {
            return new RPCStoreDadInt()
            {
                Key = dadInt.Key,
                Value = dadInt.Value,
                Epoch = dadInt.Epoch,
                EpochWriteVersion = dadInt.EpochWriteVersion,
            };
        }

        public static global::RPCStoreDadInt toProtobuf(RPCStoreDadInt dadInt)
        {
            return new global::RPCStoreDadInt
            {
                Key = dadInt.Key,
                Value = dadInt.Value,
                Epoch = dadInt.Epoch,
                EpochWriteVersion = dadInt.EpochWriteVersion,
            };
        }
    }

    internal class BroadcastMessageDTO
    {
        public static BroadcastMessage fromProtobuf(global::BroadcastMessage message)
        {
            return new BroadcastMessage()
            {
                OriginReplyLockHash = message.OriginReplyLockHash,
                DadInts = message.DadInts.Select(RPCStoreDadIntDTO.fromProtobuf).ToList()
            };
        }

        public static global::BroadcastMessage toProtobuf(BroadcastMessage message)
        {
            return new global::BroadcastMessage()
            {
                OriginReplyLockHash = message.OriginReplyLockHash,
                DadInts = { message.DadInts.Select(RPCStoreDadIntDTO.toProtobuf).ToList() }
            };
        }
    }

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
                Ok = this.serverLogic.URBBroadcast(BroadcastMessageDTO.fromProtobuf(request.Message), request.SenderId)
            });
        }

        public override Task<BEBDeliverResponse> BEBDeliver(BEBDeliverRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BEBDeliverResponse
            {
                Ok = this.serverLogic.BEBDeliver(BroadcastMessageDTO.fromProtobuf(request.Message), request.SenderId)
            });
        }
    }
}
