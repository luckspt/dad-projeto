using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.StatusHook
{
    internal class ManagerStatusHookService : global::ManagerStatusHook.ManagerStatusHookBase
    {
        private ManagerStatusHookServiceLogic serverLogic;

        public ManagerStatusHookService(ManagerStatusHookServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }
        public override Task<ExecuteResponse> Execute(ExecuteRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ExecuteResponse() { Ok = this.serverLogic.Execute(request.Id, request.Type, request.Status) });
        }
    }
}
