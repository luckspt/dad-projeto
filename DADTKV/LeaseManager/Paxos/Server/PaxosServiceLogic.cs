using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos.Server
{
    internal class PaxosServiceLogic
    {
        private TimeSlots timeSlots;
        public PaxosServiceLogic(TimeSlots timeSlots)
        {
            this.timeSlots = timeSlots;
        }

        public PromiseResponse? Prepare(int slot, int epoch, int locktableHash)
        {
            PaxosInstance? instance = timeSlots.GetPaxosInstance(slot);
            if (instance == null) return null;

            PromiseResponse? response = instance.Phase1.Prepare(epoch, locktableHash);
            if (response == null) return null; // just to be explicit

            return response;
        }

        public AcceptedResponse? Accept(int slot, int epoch, Dictionary<string, List<string>> leases)
        {
            PaxosInstance? instance = timeSlots.GetPaxosInstance(slot);
            if (instance == null) return null;

            AcceptedResponse? response = instance.Phase2.Accept(epoch, leases);
            if (response == null) return null; // just to be explicit

            return response;
        }
    }
}
