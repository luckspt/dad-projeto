using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerClientServices
{
    public class ManagerService : global::ManagerService.ManagerServiceBase
    {
        private ManagerServiceLogic serverLogic;
        public ManagerService(ManagerServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<CrashResponse> Crash(CrashRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CrashResponse { Ok = this.serverLogic.Crash() });
        }

        public override Task<StatusHookConfigResponse> StatusHookConfig(StatusHookConfigRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StatusHookConfigResponse { Ok = this.serverLogic.StatusHookConfig(request.Enabled, request.HookIntervalMs) });
        }
    }
}
