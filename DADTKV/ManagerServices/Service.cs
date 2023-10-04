using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerServices
{
    internal class Service : ManagerService.ManagerServiceBase
    {
        private ManagerServer managerServer;
        public Service(ManagerServer managerLogic)
        {
            this.managerServer = managerLogic;
        }

        public override Task<CrashResponse> Crash(CrashRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CrashResponse { Ok = this.managerServer.Crash() });
        }

        public override Task<StatusHookConfigResponse> StatusHookConfig(StatusHookConfigRequest request, ServerCallContext context)
        {
            return Task.FromResult(new StatusHookConfigResponse { Ok = this.managerServer.StatusHookConfig(request.Enabled, request.HookIntervalMs) });
        }
    }
}
