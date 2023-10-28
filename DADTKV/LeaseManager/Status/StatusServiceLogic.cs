using Common;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Status
{
    internal class StatusServiceLogic
    {
        private LeaseManager leaseManager;
        public StatusServiceLogic(LeaseManager leaseManager)
        {
            this.leaseManager = leaseManager;
        }

        public void Status()
        {
            string message = "\n\nSTATUS:";

            message += $"Slots:\n- {this.leaseManager.TimeSlots.CurrentSlot}/{this.leaseManager.TimeSlots.Slots} (current/total)";

            LeaseStore requestsBuffer = this.leaseManager.LeaseRequestsBuffer.GetBuffer();
            message += $"Requests:\n- {requestsBuffer.Count} lease requests pending ({string.Join(",", requestsBuffer.Select(x => x.Key))})";

            System.Console.WriteLine(message);
        }
    }
}
