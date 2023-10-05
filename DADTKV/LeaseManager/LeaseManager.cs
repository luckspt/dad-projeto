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
        private LeaseRequestsBuffer leaseRequestsBuffer;
        private TimeSlots timeSlots;
        private Timer? paxosTimer;

        public LeaseManager(LeaseRequestsBuffer leaseRequestsBuffer, int slots, int slotDurationMs)
        {
            this.leaseRequestsBuffer = leaseRequestsBuffer;
            this.timeSlots = new TimeSlots(slots, slotDurationMs);
        }

        public void Start()
        {
            this.paxosTimer = new Timer(this.StartPaxos!, this.timeSlots.Slots, TimeSpan.FromMilliseconds(this.timeSlots.SlotDurationMs), TimeSpan.FromMilliseconds(this.timeSlots.SlotDurationMs));
        }

        private void StartPaxos(object state)
        {
            lock (this.leaseRequestsBuffer)
            {
                try
                {
                    this.timeSlots.CreateNewPaxosInstance(this.leaseRequestsBuffer.GetBuffer());
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
