using LeaseManager.Paxos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager
{
    internal class TimeSlots
    {
        public int CurrentSlot { get; private set; }
        public int Slots { get; private set; }
        public int SlotDurationMs { get; private set; }
        private SortedList<int, PaxosInstance> paxosInstances = new SortedList<int, PaxosInstance>();

        public TimeSlots(int slots, int slotDurationMs)
        {
            this.CurrentSlot = 0;
            this.Slots = slots;
            this.SlotDurationMs = slotDurationMs;
        }

        public void CreateNewPaxosInstance(Dictionary<string, List<string>> leases, List<LMPeer> proposers, List<LMPeer> acceptors, List<LMPeer> learners)
        {
            lock (this)
            {
                // TODO: is this everything to start a new Paxos instance?
                // - it's missing the peers at least
                Console.WriteLine($"[LM] Starting new Paxos Instance (slot={this.CurrentSlot}) with {leases.Count} requests");

                PaxosInstance instance = new PaxosInstance(this.CurrentSlot, 0, leases, proposers, acceptors, learners);
                this.paxosInstances.Add(this.CurrentSlot, instance);
                this.CurrentSlot++;
            }
        }

        public PaxosInstance? GetPaxosInstance(int slot)
        {
            lock (this)
            {
                return this.paxosInstances.GetValueOrDefault(slot);
            }
        }

        public bool RemovePaxosInstance(int slot)
        {
            lock (this)
            {
                return this.paxosInstances.Remove(slot);
            }
        }
    }
}
