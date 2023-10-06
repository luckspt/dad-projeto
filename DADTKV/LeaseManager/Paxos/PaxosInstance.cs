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
            this.promises = new Promises { GreatestWriteTimestamp = 0, ReceivedCount = 0 };
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

        public void SendPrepare()
        {
            // Proposer sends a prepare
            lock (this)
            {
                // Reset values
                this.promises = new Promises { GreatestWriteTimestamp = 0, ReceivedCount = 0 };
                this.accepteds = new Accepteds { ReceivedCount = new Dictionary<int, int>() };

                PrepareRequest prepare = new PrepareRequest
                {
                    Slot = this.Slot,
                    ProposalNumber = this.proposalNumber,
                    ProposerLeasesHash = this.SelfLeases.GetHashCode(),
                };

                // TODO should this be in thread? If so, make copies of the values we are sending so they don't change (because they are references)
                this.Client.Prepare(prepare);
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
                if (this.readTimestamp >= prepare.ProposalNumber)
                    return null; // nack

                this.readTimestamp = prepare.ProposalNumber;

                return this.CraftPromise(prepare.ProposerLeasesHash);
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
                WriteTimestamp = proposalNumber,
                // .ToDictionary so its a copy and not a reference
                Value = this.value?.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value)),
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
                // We don't need to count on the proposal number because acceptors return nack if they don't promise
                this.promises.ReceivedCount++;

                // -1 because we also count
                int otherAcceptors = (this.GetAcceptors().Count - 1) / 2;
                // If we already have the majority, we don't care about the rest
                if (this.promises.ReceivedCount > otherAcceptors)
                    return false;

                // If the write timestamp is higher than the highest write timestamp we have seen, update our values
                if (promise.WriteTimestamp > this.promises.GreatestWriteTimestamp)
                {
                    this.promises.GreatestWriteTimestamp = promise.WriteTimestamp;
                    this.value = promise.Value;
                }

                if (promise.SelfLeases != null)
                {
                    // TODO merge self leases so we propose the joint value
                    //  of all responding proposers if we/they don't have some values
                }

                //  == will make it so we only accept the majority once
                if (this.promises.ReceivedCount == otherAcceptors)
                    this.ReceivedMajorityPromises();

                return true;
            }
        }

        /// <summary>
        /// Proposer notes it has received a majority of promises and can continue to Phase 2
        /// </summary>
        private void ReceivedMajorityPromises()
        {
            // TODO should this be in a thread?
            // - if it should, do we need to lock? Because this.value or this.SelfLeases may change
            this.SendAccept();
        }

        /// <summary>
        /// Proposer starts Phase 2 and sends an accept
        /// </summary>
        private void SendAccept()
        {
            // No need to lock, we are already locked from ReceivedMajorityPromises which is locked by ProcessPromise
            // - !!! if we change ReceivedMajorityPromises to a thread, beware of locking!!!
            Dictionary<string, List<string>> toPropose
                = this.promises.GreatestWriteTimestamp != 0 ? this.value! : this.SelfLeases;

            AcceptRequest accept = new AcceptRequest
            {
                Slot = this.Slot,
                ProposalNumber = proposalNumber,
                Value = toPropose,
            };

            // TODO should this be in thread? If so, make copies of the values we are sending so they don't change (because they are references)
            this.Client.Accept(accept);
        }

        /// <summary>
        /// Acceptor receives an accept and processes it
        /// </summary>
        /// <returns>The Accepted</returns>
        public AcceptedResponse? ProcessAccept(AcceptRequest accept)
        {
            lock (this)
            {
                // We only accept if the proposal number is the same as the one we have promised
                if (this.readTimestamp != accept.ProposalNumber)
                    return null;

                this.value = accept.Value;
                this.writeTimestamp = accept.ProposalNumber;

                return this.CraftAccepted(accept.ProposalNumber, this.value);
            }
        }

        /// <summary>
        /// Internal method to craft an Accepted
        /// </summary>
        /// <returns>The Accepted</returns>
        private AcceptedResponse CraftAccepted(int proposalNumber, Dictionary<string, List<string>> value)
        {
            return new AcceptedResponse
            {
                Slot = this.Slot,
                ProposalNumber = proposalNumber,
                Value = value,
            };
        }

        /// <summary>
        /// Proposer receives an accepted and processes it
        /// </summary>
        public bool ProcessAccepted(AcceptedResponse accepted)
        {
            lock (this)
            {
                // Increment the number of accepted responses of this proposal number
                int count = 0;
                this.accepteds.ReceivedCount.TryGetValue(accepted.ProposalNumber, out count);
                this.accepteds.ReceivedCount[accepted.ProposalNumber] = count + 1;

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

        /// <summary>
        /// To execute when consensus is reached
        /// </summary>
        private void ConsensusReached()
        {
            // No need to lock, we are already locked from ProcessAccepted
            // TODO can we garbage collect this instance since it reached consensus?
        }
    }
}