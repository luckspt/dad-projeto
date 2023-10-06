using Common;
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
            this.CurrentSlot = 1;
            this.Slots = slots;
            this.SlotDurationMs = slotDurationMs;
        }

        public void CreateNewPaxosInstance(LeaseStore leases, List<LMPeer> proposers, List<LMPeer> acceptors, List<LMPeer> learners, int proposerPosition)
        {
            Task t;
            lock (this)
            {
                PaxosInstance instance = new PaxosInstance(this.CurrentSlot, proposerPosition, leases, proposers, acceptors, learners);
                Logger.GetInstance().Log("TimeSlots", $"Starting new Paxos Instance (slot={this.CurrentSlot}) with {leases.Count} lease requests. Proposer={instance.Proposal.IsProposer()}, ProposerPosition={proposerPosition}");

                this.paxosInstances.Add(this.CurrentSlot, instance);
                this.CurrentSlot++;

                t = new Task(() => instance.Start());
            }

            t.Start();
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
