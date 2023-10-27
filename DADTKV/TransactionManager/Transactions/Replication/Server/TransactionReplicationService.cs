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

    internal class ReplicationMessageDTO
    {
        public static ReplicationMessage fromProtobuf(global::ReplicationMessage message)
        {
            return new ReplicationMessage()
            {
                Guid = message.Guid,
                ExecutingManagerId = message.ExecutingManagerId,
                DadInts = message.DadInts.Select(RPCStoreDadIntDTO.fromProtobuf).ToList(),
                ReadDadInts = message.ReadDadInts.ToList(),
            };
        }

        public static global::ReplicationMessage toProtobuf(ReplicationMessage message)
        {
            return new global::ReplicationMessage()
            {
                Guid = message.Guid,
                ExecutingManagerId = message.ExecutingManagerId,
                DadInts = { message.DadInts.Select(RPCStoreDadIntDTO.toProtobuf).ToList() },
                ReadDadInts = { message.ReadDadInts.ToList() }
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
                Ok = this.serverLogic.URBBroadcast(ReplicationMessageDTO.fromProtobuf(request.Message), request.SenderId)
            });
        }

        public override Task<BEBDeliverResponse> BEBDeliver(BEBDeliverRequest request, ServerCallContext context)
        {
            return Task.FromResult(new BEBDeliverResponse
            {
                Ok = this.serverLogic.BEBDeliver(ReplicationMessageDTO.fromProtobuf(request.Message), request.SenderId)
            });
        }
    }
}
