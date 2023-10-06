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

        public void Start(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses)
        {
            List<LMPeer> proposers = leaseManagersAddresses.Select(address => new LMPeer(address)).ToList();
            List<LMPeer> acceptors = proposers.ToList();
            List<LMPeer> learners = proposers.ToList()
                .Concat(transactionManagersAddresses.Select(address => new LMPeer(address)).ToList())
                .ToList();

            this.paxosTimer = new Timer((object state) => this.StartPaxos(proposers, acceptors, learners), this.TimeSlots.Slots, TimeSpan.FromMilliseconds(this.TimeSlots.SlotDurationMs), TimeSpan.FromMilliseconds(this.TimeSlots.SlotDurationMs));
        }

        private void StartPaxos(List<LMPeer> proposers, List<LMPeer> acceptors, List<LMPeer> learners)
        {
            lock (this.leaseRequestsBuffer)
            {
                try
                {
                    this.TimeSlots.CreateNewPaxosInstance(this.leaseRequestsBuffer.GetBuffer(), proposers, acceptors, learners);
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
