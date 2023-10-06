using Common;
using LeaseManager.Paxos.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    public struct AcceptRequest
    {
        public int Slot;
        public int ProposalNumber;
        public LeaseStore Value;
    }

    internal class AcceptRequestDTO
    {
        public static AcceptRequest fromProtobuf(global::AcceptRequest response)
        {
            return new AcceptRequest
            {
                Slot = response.Slot,
                ProposalNumber = response.ProposalNumber,
                Value = TmLeasesDTO.fromProtobuf(response.Value)
            };
        }

        public static global::AcceptRequest toProtobuf(AcceptRequest response)
        {
            return new global::AcceptRequest
            {
                Slot = response.Slot,
                ProposalNumber = response.ProposalNumber,
                Value = { TmLeasesDTO.toProtobuf(response.Value) }
            };
        }
    }

    public struct AcceptedResponse
    {
        public int Slot;
        public int ProposalNumber;
        public LeaseStore? Value;
    }

    public struct Accepteds
    {
        /// <summary>
        /// Key is the proposal number, value is the number of accepteds received for that proposal number.
        /// </summary>
        public Dictionary<int, int> ReceivedCount;
    }

    internal class AcceptedResponseDTO
    {
        public static AcceptedResponse? fromProtobuf(global::AcceptedResponse? response)
        {
            if (response == null) return null;

            return new AcceptedResponse
            {
                Slot = response.Slot,
                ProposalNumber = response.ProposalNumber,
                Value = response.Value == null ? null : TmLeasesDTO.fromProtobuf(response.Value)
            };
        }

        public static global::AcceptedResponse? toProtobuf(AcceptedResponse? response)
        {
            if (response == null) return null;

            return new global::AcceptedResponse
            {
                Slot = response.Value.Slot,
                ProposalNumber = response.Value.ProposalNumber,
                Value = { response.Value.Value == null ? null : TmLeasesDTO.toProtobuf(response.Value.Value) }
            };
        }
    }
}
