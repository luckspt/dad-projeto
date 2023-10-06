using Common;
using Google.Protobuf.WellKnownTypes;
using LeaseManager.Paxos.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos
{
    public struct PrepareRequest
    {
        public int Slot;
        public int ProposalNumber;
        public string ProposerLeasesHash;
    }

    internal class PrepareRequestDTO
    {
        public static PrepareRequest fromProtobuf(global::PrepareRequest request)
        {
            return new PrepareRequest
            {
                Slot = request.Slot,
                ProposalNumber = request.ProposalNumber,
                ProposerLeasesHash = request.ProposerLeasesHash
            };
        }

        public static global::PrepareRequest toProtobuf(PrepareRequest request)
        {
            return new global::PrepareRequest
            {
                Slot = request.Slot,
                ProposalNumber = request.ProposalNumber,
                ProposerLeasesHash = request.ProposerLeasesHash
            };
        }
    }


    public struct PromiseResponse
    {
        public int Slot;
        public int WriteTimestamp;
        public LeaseStore? Value;
        public LeaseStore SelfLeases;
    }

    public struct Promises
    {
        public int GreatestWriteTimestamp;
        public int ReceivedCount;
    }

    internal class PromiseResponseDTO
    {
        public static PromiseResponse? fromProtobuf(global::PromiseResponse? response)
        {
            if (response == null) return null;

            return new PromiseResponse
            {
                Slot = response.Slot,
                WriteTimestamp = response.WriteTimestamp,
                Value = response.Value == null ? null : TmLeasesDTO.fromProtobuf(response.Value),
                SelfLeases = response.SelfLeases == null ? null : TmLeasesDTO.fromProtobuf(response.SelfLeases)
            };
        }

        public static global::PromiseResponse? toProtobuf(PromiseResponse? response)
        {
            if (response == null) return null;

            global::PromiseResponse promise = new global::PromiseResponse
            {
                Slot = response.Value.Slot,
                WriteTimestamp = response.Value.WriteTimestamp,
                SelfLeases = { TmLeasesDTO.toProtobuf(response.Value.SelfLeases) }
            };

            if (response.Value.Value != null)
                promise.Value.AddRange(TmLeasesDTO.toProtobuf(response.Value.Value));

            return promise;
        }
    }
}
