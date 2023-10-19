using Common;
using DADTKV;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Leases.LeaseRequesting
{
    internal class DadIntListDTO
    {
        public static List<DadInt> fromProtobuf(RepeatedField<RPCDadInt> rpcDadInts)
        {
            return rpcDadInts
                    .Select(dadint => new DadInt(dadint.Key, dadint.Value))
                    .ToList();
        }

        public static RepeatedField<RPCDadInt> toProtobuf(List<DadInt> dadInts)
        {
            RepeatedField<RPCDadInt> rpcDadInts = new RepeatedField<RPCDadInt>();
            rpcDadInts.AddRange(
                dadInts
                .Select(dadint => new RPCDadInt
                {
                    Key = dadint.Key,
                    Value = dadint.Value
                }));

            return rpcDadInts;
        }
    }


    internal class TransactionsServiceClient
    {
        public List<DadInt> ApplyTransaction(Peer server, List<string> keysToRead, List<DadInt> keysToWrite)
        {
            Logger.GetInstance().Log($"ClientTransactionsService", $"Reading {string.Join(", ", keysToRead)} and writing {string.Join(", ", keysToWrite)}");

            try
            {
                ApplyTransactionResponse response = this.GetClient(server.Address).ApplyTransaction(new ApplyTransactionRequest
                {
                    KeysToRead = { keysToRead },
                    KeysToWrite = { DadIntListDTO.toProtobuf(keysToWrite) },
                });

                return DadIntListDTO.fromProtobuf(response.ValuesRead);
            }
            catch (RpcException e)
            {
                Logger.GetInstance().Log($"ClientTransactionsService", $"Error: {e.Message}");
                return new List<DadInt>() { DadInt.CreateAborted() };
            }
        }

        private TransactionsService.TransactionsServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new TransactionsService.TransactionsServiceClient(serverChannel);
        }
    }
}
