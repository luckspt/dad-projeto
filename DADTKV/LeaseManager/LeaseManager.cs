using Common;
using LeaseManager.Leasing.Requesting;
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
        public LeaseRequestsBuffer LeaseRequestsBuffer { get; }
        private Timer? paxosTimer;

        public LeaseManager(LeaseRequestsBuffer leaseRequestsBuffer, int slots, int slotDurationMs)
        {
            this.TimeSlots = new TimeSlots(slots, slotDurationMs);
            this.LeaseRequestsBuffer = leaseRequestsBuffer;
        }

        public void Start(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses, int proposerPosition)
        {
            List<Peer> proposers = leaseManagersAddresses.Select(address => Peer.FromString(address)).ToList();
            List<Peer> acceptors = proposers.ToList();
            List<Peer> learners = transactionManagersAddresses.Select(address => Peer.FromString(address)).ToList();

            // TODO REMOVE (this way it start right away)
            // new Task(() => this.StartPaxos(proposers, acceptors, learners, proposerPosition)).Start();
            // --
            this.paxosTimer = new Timer((object state) => this.StartPaxos(proposers, acceptors, learners, proposerPosition), this.TimeSlots.Slots, TimeSpan.FromMilliseconds(this.TimeSlots.SlotDurationMs), TimeSpan.FromMilliseconds(this.TimeSlots.SlotDurationMs));
        }

        private void StartPaxos(List<Peer> proposers, List<Peer> acceptors, List<Peer> learners, int proposerPosition)
        {
            lock (this.LeaseRequestsBuffer)
            {
                try
                {
                    if (this.TimeSlots.CurrentSlot >= this.TimeSlots.Slots)
                    {
                        this.paxosTimer?.Dispose();
                        this.paxosTimer = null;
                        return;
                    }

                    // DON'T DO THIS: A REPLICA MAY BE EMPTY (because it couldn't be reached by TMs) BUT OTHERS MAY HAVE LEASES
                    // if (this.leaseRequestsBuffer.GetBuffer().Count == 0)
                    // return;

                    this.TimeSlots.CreateNewPaxosInstance(this.LeaseRequestsBuffer.GetBuffer(), proposers, acceptors, learners, proposerPosition);
                    this.LeaseRequestsBuffer.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
