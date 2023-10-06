using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos.Server
{
    public class Proposal
    {
        public int Number
        {
            get => this.number;
            set
            {
                this.number = value;

                // Status for GUI
                Program.ManagerClient.Status = "Paxos" + (this.IsProposer() ? "Proposer" : "Acceptor");
            }
        }
        private int number;
        private int proposerCount;
        private int selfPosition;

        /// <summary>
        /// Proposal number manager
        /// </summary>
        /// <param name="selfPosition">Must start at 1 (it is not an index)</param>
        /// <param name="proposerCount">The amount of proposers</param>
        public Proposal(int selfPosition, int proposerCount)
        {
            this.selfPosition = selfPosition;
            this.proposerCount = proposerCount;
            this.Number = 1; // always starts at 1
        }

        /// <summary>
        /// Move to the next proposal number where I'm the proposer.
        /// </summary>
        public void NextProposalNumber()
        {
            // A = 1, B = 2, C = 3, D = 4, E = 5, F = 6, G = 7
            // then A = 8, B = 9, C = 10, D = 11, E = 12, F = 13, G = 14

            // When this method is called by F and number is 10 (C), then it will be 13.
            // - difference between F and C is 3, so we add 3 to the number
            // When this method is called by B and number is 10 (C), then it will be 16.
            // - difference between C and B is -1, so we add a new round (+7 = 17),
            //  where the difference -1 is subtracted
            int difference = this.selfPosition - this.Number % this.proposerCount;
            if (difference < 0) difference += this.proposerCount;
            this.Number += difference;
        }

        public bool IsProposer()
        {
            return this.Number % this.proposerCount == this.selfPosition;
        }
    }
}
