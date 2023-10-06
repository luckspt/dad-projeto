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

        public PromiseResponse? Prepare(PrepareRequest request)
        {
            PaxosInstance? instance = timeSlots.GetPaxosInstance(request.Slot);
            if (instance == null) return null;

            PromiseResponse? response = instance.ProcessPrepare(request);
            if (response == null) return null; // just to be explicit

            return response;
        }

        public AcceptedResponse? Accept(AcceptRequest request)
        {
            PaxosInstance? instance = timeSlots.GetPaxosInstance(request.Slot);
            if (instance == null) return null;

            AcceptedResponse? response = instance.ProcessAccept(request);
            if (response == null) return null; // just to be explicit

            return response;
        }
    }
}
