using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.LeaseRequesting
{
    internal class LeaseRequestingServiceLogic
    {
        public LeaseRequestsBuffer leaseBuffer;
        public LeaseRequestingServiceLogic(LeaseRequestsBuffer leaseBuffer)
        {
            this.leaseBuffer = leaseBuffer;
        }

        public bool RequestLeases(string requesterTMId, List<string> keys)
        {
            this.leaseBuffer.AddRange(requesterTMId, keys);
            return true;
        }
    }
}
