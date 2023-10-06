using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using LeaseManager.Paxos.Client;
using LeaseManager.Paxos.Server;

namespace LeaseManager.Paxos
{
    public class TMLeases
    {
        public String Key { get; }
        public List<string> TmIds { get; }
    }

    public class LMPeer
    {
        public string Address { get; }

        public LMPeer(string address)
        {
            this.Address = address;
        }
    }

    public class PaxosInstance
    {
        public readonly int Slot;
        public readonly PaxosServiceClient Client;
        public readonly Dictionary<string, List<string>> SelfLeases;
        public Proposal Proposal { get; }
        public List<LMPeer> Proposers { get; }
        public List<LMPeer> Acceptors { get; }
        public List<LMPeer> Learners { get; }

        private Dictionary<string, List<string>>? value;
        private int writeTimestamp;
        private int readTimestamp;

        private Promises promises;
        private Accepteds accepteds;

        public PaxosInstance(int slot, int proposerPosition, Dictionary<string, List<string>> selfLeases, List<LMPeer> proposers, List<LMPeer> acceptors, List<LMPeer> learners)
        {
            this.Slot = slot;
            this.Client = new PaxosServiceClient(this);
            this.SelfLeases = selfLeases;
            this.Proposers = proposers;
            this.Acceptors = acceptors;
            this.Learners = learners;
            this.Proposal = new Proposal(proposerPosition, this.Proposers.Count);
            this.value = null;
            this.writeTimestamp = 0;
            this.readTimestamp = 0;
            this.promises = new Promises { GreatestWriteTimestamp = 0, ReceivedCount = 0 };
            this.accepteds = new Accepteds { ReceivedCount = new Dictionary<int, int>() };
        }

        public void Start()
        {
            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"Starting (proposer={this.Proposal.IsProposer()})");
            if (this.Proposal.IsProposer())
                this.SendPrepare();
        }

        /// <summary>
        /// Start a new proposal round
        /// </summary>
        private void NewProposalRound()
        {
            lock (this)
            {
                this.Proposal.NextProposalNumber();
                Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"New Proposal Round");
                // TODO Will this cause issues because we are already locked?
                this.SendPrepare();
            }
        }

        /// <summary>
        /// Start Phase 1 and send a prepare
        /// </summary>
        private void SendPrepare()
        {
            // Proposer sends a prepare
            lock (this)
            {
                Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"SendPrepare");
                // Reset values
                this.promises = new Promises { GreatestWriteTimestamp = 0, ReceivedCount = 0 };
                this.accepteds = new Accepteds { ReceivedCount = new Dictionary<int, int>() };

                PrepareRequest prepare = new PrepareRequest
                {
                    Slot = this.Slot,
                    ProposalNumber = this.Proposal.Number,
                    ProposerLeasesHash = this.SelfLeases.GetHashCode(),
                };

                // If there's a higher proposal number, we will receive a nack and start a new proposal round
                if (!this.Client.Prepare(prepare))
                    this.NewProposalRound();
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
                Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"ProcessPrepare (readTimestamp={this.readTimestamp})");
                if (this.readTimestamp >= prepare.ProposalNumber)
                {
                    Logger.GetInstance().Log($"Paxos.{this.Slot}", $"ProcessPrepare NACK prepare because readTimestamp > proposalNumber");
                    return null; // nack
                }

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
            int selfHash = this.SelfLeases.GetHashCode();
            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"CraftPromise (writeTimestamp={this.writeTimestamp}, proposerHash={proposerHash}, selfHash={selfHash})");

            // No need to lock, we are already locked from ProcessPrepare
            PromiseResponse promise = new PromiseResponse
            {
                Slot = this.Slot,
                WriteTimestamp = this.writeTimestamp,
                // .ToDictionary so its a copy and not a reference
                Value = this.value?.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value)),
            };

            if (selfHash != proposerHash)
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
                int neededToAccept = (this.Acceptors.Count - 1) / 2;

                Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"ProcessPromise (promises.ReceivedCount={this.promises.ReceivedCount}, neededToAccept={neededToAccept}, promise.writeTimestamp={promise.WriteTimestamp}, greatestWriteTimestamp={this.promises.GreatestWriteTimestamp}, receivedSelfLeases={promise.SelfLeases != null})");
                // If we already have the majority, we don't care about the rest
                if (this.promises.ReceivedCount > neededToAccept)
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
                if (this.promises.ReceivedCount == neededToAccept)
                    this.ReceivedMajorityPromises();

                return true;
            }
        }

        /// <summary>
        /// Proposer notes it has received a majority of promises and can continue to Phase 2
        /// </summary>
        private void ReceivedMajorityPromises()
        {
            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"ReceivedMajorityPromises");
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

            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"SendAccept (promises.GreatestWriteTimestamp={this.promises.GreatestWriteTimestamp})");
            AcceptRequest accept = new AcceptRequest
            {
                Slot = this.Slot,
                ProposalNumber = this.Proposal.Number,
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
                Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"ProcessAccept (readTimestamp={this.readTimestamp}, oldWriteTimestamp={this.writeTimestamp})");
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
                int neededToAccept = (this.Acceptors.Count - 1) / 2;

                Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"ProcessAccepted (acceptedsReceivedForProposalNumber={count}, neededToAccept={neededToAccept})");

                if (count >= neededToAccept)
                {
                    //  == will make it so we only accept the majority once
                    if (count == neededToAccept)
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
            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"Concensus Reached!");
            // No need to lock, we are already locked from ProcessAccepted
            // TODO can we garbage collect this instance since it reached consensus?
        }
    }
}