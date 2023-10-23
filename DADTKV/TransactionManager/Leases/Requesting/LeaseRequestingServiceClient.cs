using Common;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager.Leases.LeaseRequesting
{
    public struct RequestLeasesRequest
    {
        public List<string> LeaseKeys;
        public string RequesterTMId;
    }

    internal class RequestLeasesRequestDTO
    {
        public static RequestLeasesRequest? fromProtobuf(global::RequestLeasesRequest? request)
        {
            if (request == null) return null;

            return new RequestLeasesRequest
            {
                LeaseKeys = request.LeaseKeys.ToList(),
                RequesterTMId = request.RequesterTMId,
            };
        }

        public static global::RequestLeasesRequest? toProtobuf(RequestLeasesRequest? request)
        {
            if (request == null) return null;

            global::RequestLeasesRequest protobuf = new global::RequestLeasesRequest
            {
                LeaseKeys = { request.Value.LeaseKeys },
                RequesterTMId = request.Value.RequesterTMId,
            };

            return protobuf;
        }
    }


    internal class LeaseRequestingServiceClient
    {
        private Leasing leasing;

        public LeaseRequestingServiceClient(Leasing leasing)
        {
            this.leasing = leasing;
        }

        public bool RequestLeases(RequestLeasesRequest request)
        {
            Logger.GetInstance().Log($"ClientRequestLeases.{this.leasing.NextEpoch}", $"Requesting leases (leases=[{string.Join(", ", request.LeaseKeys)}])");
            List<Task<global::RequestLeasesResponse>> responses = new List<Task<global::RequestLeasesResponse>>();
            foreach (Peer lm in this.leasing.LeaseManagers)
            {
                // First we send all the requests
                // TODO handle when there is an error when sending the request (maybe it's just on receiving the response?)
                Task<global::RequestLeasesResponse> task = new Task<global::RequestLeasesResponse>(() =>
                {
                    try
                    {
                        return this.GetClient(lm.Address).RequestLeases(RequestLeasesRequestDTO.toProtobuf(request));
                    }
                    catch (RpcException e)
                    {
                        Logger.GetInstance().Log($"ClientRequestLeases.{this.leasing.NextEpoch}", $"ERROR ON LeaseRequestingService.RequestLeasesleases (sending thread) {e.Message}");
                        return null;
                    }
                });

                task.Start();
                responses.Add(task);
            };

            // Then we wait for all the responses
            int acks = 0;
            foreach (Task<global::RequestLeasesResponse> response in responses)
            {
                try
                {
                    // TODO handle when there is no response so we don't wait forever
                    response.Wait();
                    global::RequestLeasesResponse? ackResponse = response.Result;

                    // We receive a null, meaning the acceptor already promised to a higher value.
                    if (ackResponse != null && ackResponse.Ok)
                    {
                        acks++;

                        if (acks >= this.leasing.LeaseManagers.Count / 2)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.GetInstance().Log($"ClientRequestLeases.{this.leasing.NextEpoch}", $"ERROR ON LeaseRequestingService.RequestLeasesleases (receiving) {e.Message}");
                }
            }

            return true;
        }

        private LeaseRequestingService.LeaseRequestingServiceClient GetClient(string address)
        {
            GrpcChannel serverChannel = GrpcChannel.ForAddress(address);
            return new LeaseRequestingService.LeaseRequestingServiceClient(serverChannel);
        }
    }
}
