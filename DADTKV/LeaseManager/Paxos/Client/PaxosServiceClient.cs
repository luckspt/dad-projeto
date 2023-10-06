using Grpc.Core;
using Grpc.Net.Client;
using LeaseManager.Paxos.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Paxos.Client
{
    public class PaxosServiceClient
    {
        private PaxosInstance instance;

        public PaxosServiceClient(PaxosInstance instance)
        {
            this.instance = instance;
        }

        public void Prepare(PrepareRequest prepare)
        {
            List<AsyncUnaryCall<global::PromiseResponse>> responses = new List<AsyncUnaryCall<global::PromiseResponse>>();
            foreach (LMPeer acceptor in instance.GetAcceptors())
            {
                // First we send all the requests
                // TODO handle when there is an error when sending the request (maybe it's just on receiving the response?)
                responses.Add(GetClient(acceptor.Address).PrepareAsync(PrepareRequestDTO.toProtobuf(prepare)));
            };

            // Then we wait for all the responses
            foreach (AsyncUnaryCall<global::PromiseResponse> response in responses)
            {
                try
                {
                    // TODO handle when there is no response so we don't wait forever
                    response.ResponseAsync.Wait();
                    global::PromiseResponse? promiseResponse = response.ResponseAsync.Result;

                    // We receive a null, meaning the acceptor already promised to a higher value.
                    // TODO: how should we handle this?
                    if (promiseResponse == null) continue;

                    this.instance.ProcessPromise(PromiseResponseDTO.fromProtobuf(promiseResponse)!.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR ON PaxosServiceClient.Prepare {e.Message}");
                }
            }
        }

        public void Accept(int epoch, Dictionary<string, List<string>> leases)
        {
            List<AsyncUnaryCall<global::AcceptedResponse>> responses = new List<AsyncUnaryCall<global::AcceptedResponse>>();
            foreach (LMPeer acceptor in instance.GetAcceptors())
            {
                AcceptRequest request = new AcceptRequest
                {
                    Slot = instance.Slot,
                    Epoch = epoch,
                    Leases = { TmLeasesDTO.toProtobuf(leases) }
                };

                // First we send all the requests
                // TODO handle when there is an error when sending the request (maybe it's just on receiving the response?)
                responses.Add(GetClient(acceptor.Address).AcceptAsync(request));
            }

            // Then we wait for all the responses
            foreach (AsyncUnaryCall<global::AcceptedResponse> response in responses)
            {
                try
                {
                    // TODO handle when there is no response so we don't wait forever
                    response.ResponseAsync.Wait();
                    global::AcceptedResponse? acceptedResponse = response.ResponseAsync.Result;

                    // We receive a null, meaning the acceptor already promised to a higher value.
                    // TODO: how should we handle this?
                    if (acceptedResponse == null) continue;

                    this.instance.ProcessAccepted(AcceptedResponseDTO.fromProtobuf(acceptedResponse)!.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR ON PaxosServiceClient.Prepare {e.Message}");
                }
            }
        }

        private PaxosService.PaxosServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new PaxosService.PaxosServiceClient(serverChannel);
        }
    }
}
