using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.LeaseRequesting
{
    internal class LeaseRequestingService : global::LeaseRequestingService.LeaseRequestingServiceBase
    {
        private LeaseRequestingServiceLogic serverLogic;

        public LeaseRequestingService(LeaseRequestingServiceLogic serverLogic)
        {
            this.serverLogic = serverLogic;
        }

        public override Task<RequestLeasesResponse> RequestLeases(RequestLeasesRequest request, ServerCallContext context)
        {
            return Task.FromResult(new RequestLeasesResponse() { Ok = this.serverLogic.RequestLeases(request.RequesterTMId, request.LeaseKeys.ToList()) });
        }
    }
}
