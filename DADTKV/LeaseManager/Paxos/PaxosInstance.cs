using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaseManager.Paxos.Client;

namespace LeaseManager.Paxos
{
    public class TMLeases
    {
        public String Key { get; }
        public List<string> TmIds { get; }
    }

    public struct LMPeer
    {
        public string Address { get; }
    }

    public class PaxosInstance
    {
        // TODO: epoch?
        public readonly int Slot;
        public readonly PaxosServiceClient Client;
        public readonly Dictionary<string, List<string>> SelfLeases;
        public List<LMPeer> peers = new List<LMPeer>();

        private int proposalNumber;
        private Dictionary<string, List<string>>? value;
        private int writeTimestamp;
        private int readTimestamp;

        private Promises promises;
        private Accepteds accepteds;

        public PaxosInstance(int slot, int proposalNumber, Dictionary<string, List<string>> selfLeases, List<LMPeer> peers)
        {
            this.Slot = slot;
            this.Client = new PaxosServiceClient(this);
            this.SelfLeases = selfLeases;
            this.peers = peers;
            this.proposalNumber = proposalNumber;
            this.value = null;
            this.writeTimestamp = 0;
            this.readTimestamp = 0;
            this.promises = new Promises { GreatestWriteEpoch = 0, ReceivedCount = 0 };
            this.accepteds = new Accepteds { ReceivedCount = new Dictionary<int, int>() };
        }

        public List<LMPeer> GetLearners()
        {
            return this.peers;
        }

        public List<LMPeer> GetProposers()
        {
            return this.peers;
        }

        public List<LMPeer> GetAcceptors()
        {
            return this.peers;
        }

        public PrepareRequest SendPrepare()
        {
            // Proposer sends a prepare
            lock (this)
            {
                // Reset values
                this.promises = new Promises { GreatestWriteEpoch = 0, ReceivedCount = 0 };
                this.accepteds = new Accepteds { ReceivedCount = new Dictionary<int, int>() };

                // TODO
                return null;
            }
        }

        /// <summary>
        /// Acceptor receives a prepare and processes it
        /// </summary>
        /// <returns>The Promise</returns>
        public PromiseResponse? ProcessPrepare(PrepareRequest prepare)
        {
            lock (this)
            {
                if (this.readTimestamp >= prepare.Epoch)
                    return null; // nack

                this.readTimestamp = prepare.Epoch;

                return this.CraftPromise(prepare.LeasesHash);
            }
        }

        /// <summary>
        /// Internal method to craft a Promise
        /// </summary>
        /// <returns>The Promise</returns>
        private PromiseResponse CraftPromise(int proposerHash)
        {
            // No need to lock, we are already locked from ProcessPrepare
            PromiseResponse promise = new PromiseResponse
            {
                Slot = this.Slot,
                WriteEpoch = proposalNumber,
                // .ToDictionary so its a copy and not a reference
                Leases = this.value?.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value)),
            };

            if (this.SelfLeases.GetHashCode() != proposerHash)
                promise.SelfLeases = this.SelfLeases;

            return promise;
        }

        /// <summary>
        /// Proposer receives a promise and processes it
        /// </summary>
        public bool ProcessPromise(PromiseResponse promise)
        {
            lock (this)
            {
                // TODO promises count on value
                this.promises.ReceivedCount++;

                // -1 because we also count
                int otherAcceptors = (this.GetAcceptors().Count - 1) / 2;
                // We only care about the majority from the acceptors, so we can ignore the rest
                if (this.promises.ReceivedCount >= otherAcceptors)
                {
                    //  == will make it so we only accept the majority once
                    if (this.promises.ReceivedCount == otherAcceptors)
                        this.ReceivedMajorityPromises();

                    return false;
                }

                // If the epoch is higher than the highest epoch we have seen, update our values
                if (promise.WriteEpoch > this.promises.GreatestWriteEpoch)
                {
                    this.promises.GreatestWriteEpoch = promise.WriteEpoch;
                    this.value = promise.Leases;
                }

                if (promise.SelfLeases != null)
                {
                    // TODO merge self leases so we propose the joint value
                    //  of all responding proposers if we/they don't have some values
                }

                return true;
            }
        }

        private void ReceivedMajorityPromises()
        {
            // No need to lock, we are already locked from ProcessPromise
            Dictionary<string, List<string>> toPropose
                = this.promises.GreatestWriteEpoch != 0 ? this.value! : this.SelfLeases;

            // TODO should this be in a thread?
            // - if it is, do we need to lock? Because this.value or this.SelfLeases may change
            this.SendAccept(this.proposalNumber, toPropose);
        }

        private void SendAccept(int epoch, Dictionary<string, List<string>> leases)
        {

        }

        public AcceptedResponse? ProcessAccept(AcceptRequest accept)
        {
            lock (this)
            {
                // We only accept if the epoch is the same as the one we have promised
                if (this.readTimestamp != accept.Epoch)
                    return null;

                this.value = accept.Leases;
                this.writeTimestamp = accept.Epoch;

                return this.CraftAccepted(accept.Epoch, this.value);
            }
        }

        private AcceptedResponse CraftAccepted(int epoch, Dictionary<string, List<string>> leases)
        {
            return new AcceptedResponse
            {
                Slot = this.Slot,
                Epoch = epoch,
                Leases = leases,
            };
        }

        public bool ProcessAccepted(AcceptedResponse accepted)
        {
            lock (this)
            {
                // Increment the number of accepted responses of this proposal number
                int count = 0;
                this.accepteds.ReceivedCount.TryGetValue(accepted.Epoch, out count);
                this.accepteds.ReceivedCount[accepted.Epoch] = count + 1;


                // -1 because we also count
                int otherAcceptors = (this.GetAcceptors().Count - 1) / 2;
                if (count >= otherAcceptors)
                {
                    //  == will make it so we only accept the majority once
                    if (count == otherAcceptors)
                        // TODO maybe consensus is not reached because of
                        // - https://en.wikipedia.org/wiki/Paxos_(computer_science)#Basic_Paxos_where_an_Acceptor_accepts_Two_Different_Values
                        this.ConsensusReached();
                    return false;
                }

                return true;
            }
        }

        private void ConsensusReached()
        {
            // No need to lock, we are already locked from ProcessAccepted
        }
    }
}