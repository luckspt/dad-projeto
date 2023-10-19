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

        public override Task<StartLeaseManagerResponse> StartLeaseManager(StartLeaseManagerRequest request, ServerCallContext context)
        {
            if (this.serverLogic.StartLeaseManagerDelegate != null)
                return Task.FromResult(new StartLeaseManagerResponse { Ok = this.serverLogic.StartLeaseManagerDelegate(request.LeaseManagersAddresses.ToList(), request.TransactionManagersAddresses.ToList(), request.ProposerPosition) });
            else
                return Task.FromResult(new StartLeaseManagerResponse { Ok = false });
        }

        public override Task<StartTransactionManagerResponse> StartTransactionManager(StartTransactionManagerRequest request, ServerCallContext context)
        {
            if (this.serverLogic.StartTransactionManagerDelegate != null)
                return Task.FromResult(new StartTransactionManagerResponse { Ok = this.serverLogic.StartTransactionManagerDelegate(request.LeaseManagersAddresses.ToList(), request.TransactionManagersAddresses.ToList()) });
            else
                return Task.FromResult(new StartTransactionManagerResponse { Ok = false });
        }

    }
}
