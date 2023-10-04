using Grpc.Core;
using Common;
using System.Text.RegularExpressions;

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
            // Start the GRPC server with all the services
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // TODO receive this in args
            Program.ManagerClient = new ManagerClientServices.ManagerClient(HostPort.FromString(args[0]), args[1], EntityType.LeaseManager);

            // Set server port
            HostPort hostPort = HostPort.FromString(args[2]);
            ServerPort serverPort = new ServerPort("0.0.0.0", hostPort.Port, ServerCredentials.Insecure);
            Program.GrpcServer = new Server
            {
                Services = {
                    ManagerService.BindService(new ManagerClientServices.ManagerService(new ManagerClientServices.ManagerServiceLogic(Program.ManagerClient))),
                },
                Ports = { serverPort }
            };

            Program.GrpcServer.Start();

            // Simulate work - TODO remove
            Program.GrpcServer.ShutdownTask.Wait();
        }
    }
}