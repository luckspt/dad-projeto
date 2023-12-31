﻿using Grpc.Core;
using Common;
using System.Text.RegularExpressions;
using TransactionManager.Leases.LeaseUpdates;
using TransactionManager.Transactions.Replication;
using TransactionManager.Transactions.Replication.Server;
using TransactionManager.Status;

namespace TransactionManager
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
            TransactionManager tm = new TransactionManager(args[1]);

            ManagerClientServices.ManagerServiceLogic managerServiceLogic = new ManagerClientServices.ManagerServiceLogic(Program.ManagerClient);
            managerServiceLogic.StartTransactionManagerDelegate = (List<string> leaseManagersAddresses, List<string> transactionManagersAddresses)
                => Program.StartTransactionManager(leaseManagersAddresses, transactionManagersAddresses, tm, args[2]);

            LeaseUpdatesServiceLogic leaseUpdatesServiceLogic = new LeaseUpdatesServiceLogic(tm);
            TransactionRunningServiceLogic transactionRunningServiceLogic = new TransactionRunningServiceLogic(tm);
            TransactionReplicationServiceLogic transactionReplicationServiceLogic = new TransactionReplicationServiceLogic(tm);

            // Set server port
            HostPort hostPort = HostPort.FromString(args[2]);
            ServerPort serverPort = new ServerPort("0.0.0.0", hostPort.Port, ServerCredentials.Insecure);
            Program.GrpcServer = new Server
            {
                Services = {
                    ManagerService.BindService(new ManagerClientServices.ManagerService(managerServiceLogic)),
                    global::LeaseUpdatesService.BindService(new Leases.LeaseUpdates.LeaseUpdatesService(leaseUpdatesServiceLogic)),
                    global::TransactionRunningService.BindService(new Transactions.Replication.TransactionRunningService(transactionRunningServiceLogic)),
                    global::TransactionReplicationService.BindService(new Transactions.Replication.Server.TransactionReplicationService(transactionReplicationServiceLogic)),
                    global::StatusService.BindService(new Status.StatusService(new Status.StatusServiceLogic(tm)))
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

        private static bool StartTransactionManager(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses, TransactionManager tm, string myAddress)
        {
            // Remove myself from the list of transaction managers
            // transactionManagersAddresses.Remove(myAddress);

            Logger.GetInstance().Log("TM", "Started!");
            tm.Start(leaseManagersAddresses, transactionManagersAddresses);
            return true;
        }
    }
}
