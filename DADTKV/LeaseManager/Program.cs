using Grpc.Core;
using Common;
using System.Text.RegularExpressions;
using LeaseManager.LeaseRequesting;
using LeaseManager.Paxos;

namespace Manager
{
    internal static class Program
    {
        public static Server GrpcServer { get; private set; }
        public static ManagerClientServices.ManagerClient ManagerClient { get; private set; }

        /// <summary>
        /// Application entrypoint
        /// </summary>
        /// <param name="args">string[] { managerAddress, entityId, entityAddress}</param>
        static void Main(string[] args)
        {
            Program.ManagerClient = new ManagerClientServices.ManagerClient(HostPort.FromString(args[0]), args[1], EntityType.LeaseManager);
            LeaseRequestsBuffer leaseRequestsBuffer = new LeaseRequestsBuffer();
            LeaseManager.LeaseManager lm = new LeaseManager.LeaseManager(leaseRequestsBuffer, 10, 1000);

            // Set server port
            HostPort hostPort = HostPort.FromString(args[2]);
            ServerPort serverPort = new ServerPort("0.0.0.0", hostPort.Port, ServerCredentials.Insecure);
            Program.GrpcServer = new Server
            {
                Services = {
                    ManagerService.BindService(new ManagerClientServices.ManagerService(new ManagerClientServices.ManagerServiceLogic(Program.ManagerClient))),
                    global::LeaseRequestingService.BindService(new LeaseManager.LeaseRequesting.LeaseRequestingService(new LeaseRequestingServiceLogic(leaseRequestsBuffer))),
                    // TODO paxos server service
                    //  pass it lm (so it can later access timeSlots (the Paxos Instances)
                },
                Ports = { serverPort }
            };

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Program.GrpcServer.Start();

            // Start the Lease Manager
            lm.Start();

            // Wait indefinitely for whatever reason
            Program.GrpcServer.ShutdownTask.Wait();
        }
    }
}