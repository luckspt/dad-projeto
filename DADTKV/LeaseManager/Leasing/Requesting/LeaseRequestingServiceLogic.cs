using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Leasing.Requesting
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
            Logger.GetInstance().Log("LeaseRequestingService", $"TM {requesterTMId} requesting leases={string.Join(",", keys)}");
            leaseBuffer.AddRange(requesterTMId, keys);
            return true;
        }
    }
}
