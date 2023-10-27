using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Grpc.Core;
using LeaseManager.Leasing.Updating;
using LeaseManager.Paxos.Client;
using LeaseManager.Paxos.Server;

namespace LeaseManager.Paxos
{
    public class TMLeases
    {
        public String Key { get; }
        public List<string> TmIds { get; }
    }

    public class PaxosInstance
    {
        public readonly int Slot;
        public readonly PaxosServiceClient Client;
        public readonly LeaseStore SelfLeases;
        public Proposal Proposal { get; }
        public List<Peer> Proposers { get; }
        public List<Peer> Acceptors { get; }
        public List<Peer> Learners { get; }

        private LeaseUpdatesServiceClient leaseUpdates;

        private LeaseStore? value;
        private int writeTimestamp;
        private int readTimestamp;

        private Promises promises;
        private Accepteds accepteds;

        public PaxosInstance(int slot, int proposerPosition, LeaseStore selfLeases, List<Peer> proposers, List<Peer> acceptors, List<Peer> learners)
        {
            this.Slot = slot;
            this.Client = new PaxosServiceClient(this);
            this.SelfLeases = selfLeases;
            this.Proposers = proposers;
            this.Acceptors = acceptors;
            this.Learners = learners;
            this.Proposal = new Proposal(proposerPosition, this.Proposers.Count);
            this.leaseUpdates = new LeaseUpdatesServiceClient(this);
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
            {
                // Sleep for a bit so it's more likely that the other LMs also started the Paxos instance
                Thread.Sleep(3000);
                this.SendPrepare();
            }
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
                    ProposerLeasesHash = this.SelfLeases.GetSHA254Hash(),
                };

                // TODO: being on a thread will most likely cause issues if we create a new proposal round.........
                new Task(() =>
                {
                    // If there's a higher proposal number, we will receive a nack and start a new proposal round
                    if (!this.Client.Prepare(prepare))
                        ;
                    // TODO let's avoid this for now
                    // this.NewProposalRound();
                })
                    .Start();
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
        private PromiseResponse CraftPromise(string proposerHash)
        {
            string selfHash = this.SelfLeases.GetSHA254Hash();
            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"CraftPromise (writeTimestamp={this.writeTimestamp}, proposerHash={proposerHash}, selfHash={selfHash}, value={this.value})");

            // No need to lock, we are already locked from ProcessPrepare
            PromiseResponse promise = new PromiseResponse
            {
                Slot = this.Slot,
                WriteTimestamp = this.writeTimestamp,
                // .ToDictionary so its a copy and not a reference
                Value = this.value?.Copy(),
            };

            if (!selfHash.Equals(proposerHash))
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
                int neededToAccept = (this.Acceptors.Count - 1) / 2 + 1;

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

                // == will make it so we only accept the majority once
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
            LeaseStore toPropose
                = this.promises.GreatestWriteTimestamp != 0 ? this.value! : this.SelfLeases;

            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"SendAccept (promises.GreatestWriteTimestamp={this.promises.GreatestWriteTimestamp}, valueHash={toPropose.GetSHA254Hash()}, value={toPropose})");
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

                this.NotifyLearners(accept);

                return this.CraftAccepted(accept.ProposalNumber, this.value);
            }
        }

        private void NotifyLearners(AcceptRequest accept)
        {
            new Task(() =>
            {
                try
                {
                    // Send LeaseUpdate (accepted) to all Learners (TMs)
                    this.leaseUpdates.LeaseUpdate(new Leasing.Updating.LeaseUpdateRequest
                    {
                        Epoch = accept.Slot,
                        Leases = accept.Value
                    });
                }
                catch (RpcException e)
                {
                    Logger.GetInstance().Log("NotifyLearners", e.Message);
                    // ..
                }
            }).Start();
        }

        /// <summary>
        /// Internal method to craft an Accepted
        /// </summary>
        /// <returns>The Accepted</returns>
        private AcceptedResponse CraftAccepted(int proposalNumber, LeaseStore value)
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
                int neededToAccept = (this.Acceptors.Count - 1) / 2 + 1;

                Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"ProcessAccepted (acceptedsReceivedForProposalNumber={count}, neededToAccept={neededToAccept})");

                if (count >= neededToAccept)
                {
                    //  == will make it so we only accept the majority once
                    if (count == neededToAccept)
                        // TODO maybe consensus is not reached because of
                        // - https://en.wikipedia.org/wiki/Paxos_(computer_science)#Basic_Paxos_where_an_Acceptor_accepts_Two_Different_Values
                        this.ConsensusReached(accepted.Value);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// To execute when consensus is reached
        /// </summary>
        private void ConsensusReached(LeaseStore value)
        {
            Logger.GetInstance().Log($"Paxos.{this.Slot}.{this.Proposal.Number}", $"Concensus Reached on {value}!");
            // No need to lock, we are already locked from ProcessAccepted
            // TODO can we garbage collect this instance since it reached consensus?
        }
    }
}