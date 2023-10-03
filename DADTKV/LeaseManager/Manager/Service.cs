using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Manager
{
    internal class Service : ManagerService.ManagerServiceBase
    {
        private ManagerLogic managerLogic;
        public Service(ManagerLogic managerLogic)
        {
            this.managerLogic = managerLogic;
        }

        public override Task<CrashResponse> Crash(CrashRequest request, ServerCallContext context)
        {
            // Plain process suicide to emulate a crash
            Environment.Exit(1);

            return Task.FromResult(new CrashResponse { Ok = true });
        }

        public override Task<StatusHookConfigResponse> StatusHookConfig(StatusHookConfigRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StatusHookConfigResponse { Ok = this.managerLogic.StatusHookConfig(request.Enabled, request.HookIntervalMs) });
        }
    }
}
