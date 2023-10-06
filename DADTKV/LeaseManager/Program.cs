using Grpc.Core;
using Common;
using System.Text.RegularExpressions;
using LeaseManager.LeaseRequesting;
using LeaseManager.Paxos.Server;

namespace Manager
{
    internal static class Program
    {
        public static Server GrpcServer { get; private set; }
        public static ManagerClientServices.ManagerClient ManagerClient { get; private set; }

        /// <summary>
        /// Application entrypoint
        /// </summary>
        /// <param name="args">string[] { managerAddress, entityId, entityAddress, slots, slotDurationMs }</param>
        static void Main(string[] args)
        {
            Logger.GetInstance().Log("LM", "Starting Lease Manager...");

            Program.ManagerClient = new ManagerClientServices.ManagerClient(HostPort.FromString(args[0]), args[1], EntityType.LeaseManager);
            LeaseRequestsBuffer leaseRequestsBuffer = new LeaseRequestsBuffer();
            LeaseManager.LeaseManager lm = new LeaseManager.LeaseManager(leaseRequestsBuffer, int.Parse(args[3]), int.Parse(args[4]));

            ManagerClientServices.ManagerServiceLogic managerServiceLogic = new ManagerClientServices.ManagerServiceLogic(Program.ManagerClient);
            managerServiceLogic.StartLeaseManagerDelegate = (List<string> leaseManagersAddresses, List<string> transactionManagersAddresses, int proposerPosition)
                => Program.StartLeaseManager(leaseManagersAddresses, transactionManagersAddresses, proposerPosition, lm, args[0]);

            // Set server port
            HostPort hostPort = HostPort.FromString(args[2]);
            ServerPort serverPort = new ServerPort("0.0.0.0", hostPort.Port, ServerCredentials.Insecure);
            Program.GrpcServer = new Server
            {
                Services = {
                    ManagerService.BindService(new ManagerClientServices.ManagerService(managerServiceLogic)),
                    global::LeaseRequestingService.BindService(new LeaseManager.LeaseRequesting.LeaseRequestingService(new LeaseRequestingServiceLogic(leaseRequestsBuffer))),
                    global::PaxosService.BindService(new LeaseManager.Paxos.Server.PaxosService(new PaxosServiceLogic(lm.TimeSlots))),
                },
                Ports = { serverPort }
            };

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Program.GrpcServer.Start();
            Logger.GetInstance().Log("gRPC", $"Server Started - I am {args[2]}");

            // Starting Paxos is made remotely by the Manager so we know about the peers
            // it will call StartLeaseManager

            // Wait indefinitely
            Program.GrpcServer.ShutdownTask.Wait();
        }

        private static bool StartLeaseManager(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses, int proposerPosition, LeaseManager.LeaseManager lm, string myAddress)
        {
            // Remove myself from the list of lease managers
            leaseManagersAddresses.Remove(myAddress);

            lm.Start(leaseManagersAddresses, transactionManagersAddresses, proposerPosition);
            return true;
        }
    }
}
