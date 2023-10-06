using LeaseManager.LeaseRequesting;
using LeaseManager.Paxos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager
{
    internal class LeaseManager
    {
        public TimeSlots TimeSlots { get; }
        private LeaseRequestsBuffer leaseRequestsBuffer;
        private Timer? paxosTimer;

        public LeaseManager(LeaseRequestsBuffer leaseRequestsBuffer, int slots, int slotDurationMs)
        {
            this.TimeSlots = new TimeSlots(slots, slotDurationMs);
            this.leaseRequestsBuffer = leaseRequestsBuffer;
        }

        public void Start(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses, int proposerPosition)
        {
            List<LMPeer> proposers = leaseManagersAddresses.Select(address => new LMPeer(address)).ToList();
            List<LMPeer> acceptors = proposers.ToList();
            List<LMPeer> learners = proposers.ToList()
                .Concat(transactionManagersAddresses.Select(address => new LMPeer(address)).ToList())
                .ToList();

            // TODO REMOVE
            new Task(() => this.StartPaxos(proposers, acceptors, learners, proposerPosition)).Start();
            // --
            this.paxosTimer = new Timer((object state) => this.StartPaxos(proposers, acceptors, learners, proposerPosition), this.TimeSlots.Slots, TimeSpan.FromMilliseconds(this.TimeSlots.SlotDurationMs), TimeSpan.FromMilliseconds(this.TimeSlots.SlotDurationMs));
        }

        private void StartPaxos(List<LMPeer> proposers, List<LMPeer> acceptors, List<LMPeer> learners, int proposerPosition)
        {
            lock (this.leaseRequestsBuffer)
            {
                try
                {
                    if (this.TimeSlots.CurrentSlot >= this.TimeSlots.Slots)
                    {
                        this.paxosTimer?.Dispose();
                        this.paxosTimer = null;
                        return;
                    }

                    // if (this.leaseRequestsBuffer.GetBuffer().Count == 0)
                    // return;

                    this.TimeSlots.CreateNewPaxosInstance(this.leaseRequestsBuffer.GetBuffer(), proposers, acceptors, learners, proposerPosition);
                    this.leaseRequestsBuffer.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
