using Grpc.Core;
using Common;
using System.Text.RegularExpressions;
using TransactionManager.Leases;
using TransactionManager.Leases.LeaseUpdates;
using TransactionManager.Transactions;

namespace Manager
{
    internal static class Program
    {
        public static Server GrpcServer { get; private set; }
        public static ManagerClientServices.ManagerClient ManagerClient { get; private set; }

        /// <summary>
        /// Application entrypoint
        /// </summary>
        /// <param name="args">string[] { managerAddress, entityId, entityAddress }</param>
        static void Main(string[] args)
        {
            Logger.GetInstance().Log("TM", "Starting Transaction Manager...");

            Program.ManagerClient = new ManagerClientServices.ManagerClient(HostPort.FromString(args[0]), args[1], EntityType.TransactionManager);
            TransactionManager.TransactionManager tm = new TransactionManager.TransactionManager(args[1]);

            ManagerClientServices.ManagerServiceLogic managerServiceLogic = new ManagerClientServices.ManagerServiceLogic(Program.ManagerClient);
            managerServiceLogic.StartTransactionManagerDelegate = (List<string> leaseManagersAddresses, List<string> transactionManagersAddresses)
                => Program.StartTransactionManager(leaseManagersAddresses, transactionManagersAddresses, tm, args[2]);

            LeaseUpdatesServiceLogic leaseUpdatesServiceLogic = new LeaseUpdatesServiceLogic(tm.Leasing);
            TransactionRunningServiceLogic transactionRunningServiceLogic = new TransactionRunningServiceLogic(tm);

            // Set server port
            HostPort hostPort = HostPort.FromString(args[2]);
            ServerPort serverPort = new ServerPort("0.0.0.0", hostPort.Port, ServerCredentials.Insecure);
            Program.GrpcServer = new Server
            {
                Services = {
                    ManagerService.BindService(new ManagerClientServices.ManagerService(managerServiceLogic)),
                    global::LeaseUpdatesService.BindService(new TransactionManager.Leases.LeaseUpdates.LeaseUpdatesService(leaseUpdatesServiceLogic)),
                    global::TransactionRunningService.BindService(new TransactionManager.Transactions.TransactionRunningService(transactionRunningServiceLogic))
                },
                Ports = { serverPort }
            };

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            Program.GrpcServer.Start();
            Logger.GetInstance().Log("gRPC", $"Server Started - I am {args[1]} at {args[2]}");


            // Starting is made remotely by the Manager so we know about the peers
            // it will call StartTransactionManager

            // Wait indefinitely
            Program.GrpcServer.ShutdownTask.Wait();
        }

        private static bool StartTransactionManager(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses, TransactionManager.TransactionManager tm, string myAddress)
        {
            // Remove myself from the list of transaction managers
            transactionManagersAddresses.Remove(myAddress);

            tm.Start(leaseManagersAddresses, transactionManagersAddresses);
            return true;
        }
    }
}
