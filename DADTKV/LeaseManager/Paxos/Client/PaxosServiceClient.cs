using Common;
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

        public bool Prepare(PrepareRequest prepare)
        {
            Logger.GetInstance().Log($"ClientPrepare.{prepare.Slot}", $"Sending Prepare (proposal={prepare.ProposalNumber}, hash={prepare.ProposerLeasesHash})");
            List<Task<global::PromiseResponse>> responses = new List<Task<global::PromiseResponse>>();
            foreach (LMPeer acceptor in instance.Acceptors)
            {
                // First we send all the requests
                // TODO handle when there is an error when sending the request (maybe it's just on receiving the response?)
                Task<global::PromiseResponse> task = new Task<global::PromiseResponse>(() =>
                {
                    try
                    {
                        return GetClient(acceptor.Address).Prepare(PrepareRequestDTO.toProtobuf(prepare));
                    }
                    catch (RpcException e)
                    {
                        Logger.GetInstance().Log($"ClientPrepare.{prepare.Slot}.{prepare.ProposalNumber}", $"ERROR ON PaxosServiceClient.Prepare (sending thread) {e.Message}");
                        return null;
                    }
                });

                task.Start();
                responses.Add(task);
            };

            // Then we wait for all the responses
            foreach (Task<global::PromiseResponse> response in responses)
            {
                try
                {
                    // TODO handle when there is no response so we don't wait forever
                    response.Wait();
                    global::PromiseResponse? promiseResponse = response.Result;

                    // We receive a null, meaning the acceptor already promised to a higher value.
                    if (promiseResponse == null) return false;

                    this.instance.ProcessPromise(PromiseResponseDTO.fromProtobuf(promiseResponse)!.Value);
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Log($"ClientPrepare.{prepare.Slot}.{prepare.ProposalNumber}", $"ERROR ON PaxosServiceClient.Prepare (receiving) {e.Message}");
                }
            }

            return true;
        }

        public bool Accept(AcceptRequest accept)
        {
            Logger.GetInstance().Log($"ClientAccept.{accept.Slot}", $"Sending Accept (proposal={accept.ProposalNumber})");
            List<Task<global::AcceptedResponse>> responses = new List<Task<global::AcceptedResponse>>();
            foreach (LMPeer acceptor in instance.Acceptors)
            {
                // First we send all the requests
                // TODO handle when there is an error when sending the request (maybe it's just on receiving the response?)
                Task<global::AcceptedResponse> task = new Task<global::AcceptedResponse>(() =>
                {
                    try
                    {
                        return GetClient(acceptor.Address).Accept(AcceptRequestDTO.toProtobuf(accept));
                    }
                    catch (RpcException e)
                    {
                        Logger.GetInstance().Log($"ClientAccept.{accept.Slot}.{accept.ProposalNumber}", $"ERROR ON PaxosServiceClient.Accept (sending thread) {e.Message}");
                        return null;
                    }
                });

                task.Start();
                responses.Add(task);
            }

            // Then we wait for all the responses
            foreach (Task<global::AcceptedResponse> response in responses)
            {
                try
                {
                    // TODO handle when there is no response so we don't wait forever
                    response.Wait();
                    global::AcceptedResponse? acceptedResponse = response.Result;

                    // We receive a null, meaning the acceptor already promised to a higher value.
                    if (acceptedResponse == null) return false;

                    this.instance.ProcessAccepted(AcceptedResponseDTO.fromProtobuf(acceptedResponse)!.Value);
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Log($"ClientAccept.{accept.Slot}", $"ERROR ON PaxosServiceClient.Accept (receiving) {e.Message}");
                }
            }

            return true;
        }

        private PaxosService.PaxosServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new PaxosService.PaxosServiceClient(serverChannel);
        }
    }
}
