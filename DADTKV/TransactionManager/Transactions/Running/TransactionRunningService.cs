using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Transactions.Replication
{
    internal class TransactionRunningService : global::TransactionRunningService.TransactionRunningServiceBase
    {
        private TransactionRunningServiceLogic serverLogic;

        public TransactionRunningService(TransactionRunningServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<RunTransactionResponse> RunTransaction(RunTransactionRequest request, ServerCallContext context)
        {
            return Task.FromResult(new RunTransactionResponse()
            {
                ValuesRead = { this.serverLogic.RunTransaction(
                        request.ClientId,
                        request.KeysToRead.Select(key => new ReadOperation(key)).ToList(),
                        request.KeysToWrite.Select(x => new WriteOperation(x.Key, x.Value)).ToList()
                    )
                    .Select(x => new RPCDadInt{Key = x.Key, Value=x.Value })
                    .ToList()
                }
            });
        }
    }
}
