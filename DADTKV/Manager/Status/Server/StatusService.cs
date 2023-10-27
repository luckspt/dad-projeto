using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.Status.Server
{
    internal class StatusService : global::StatusService.StatusServiceBase
    {
        private StatusServiceLogic serverLogic;
        public StatusService(StatusServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<Empty> Status(Empty request, ServerCallContext context)
        {
            return base.Status(request, context);
        }
    }
}
