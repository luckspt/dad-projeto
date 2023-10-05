using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    internal class PaxosServiceLogic
    {
        private TimeSlots timeSlots;
        public PaxosServiceLogic(TimeSlots timeSlots)
        {
            this.timeSlots = timeSlots;
        }

        #region Phase 1
        // TODO add locktableHash
        public bool Prepare(int slot, int epoch, string? locktableHash)
        {
            PaxosInstance? instance = this.timeSlots.GetPaxosInstance(slot);
            if (instance == null) return false;
            return instance.Phase1.Prepare(epoch);
        }

        public bool Promise(int slot, int epoch, Dictionary<string, List<string>> leases)
        {
            PaxosInstance? instance = this.timeSlots.GetPaxosInstance(slot);
            if (instance == null) return false;
            return instance.Phase1.Promise(epoch, leases);
        }
        #endregion

        #region Phase 2
        public bool Accept(int slot, int epoch, Dictionary<string, List<string>> leases)
        {
            PaxosInstance? instance = this.timeSlots.GetPaxosInstance(slot);
            if (instance == null) return false;
            return instance.Phase2.Accept(epoch, leases);
        }

        public bool Accepted(int slot, int epoch, Dictionary<string, List<string>> leases)
        {
            PaxosInstance? instance = this.timeSlots.GetPaxosInstance(slot);
            if (instance == null) return false;
            return instance.Phase2.Accepted(epoch, leases);
        }
        #endregion
    }
}
