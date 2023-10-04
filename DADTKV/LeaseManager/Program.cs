using Grpc.Core;
using ManagerClientServices;

namespace Manager
{
    internal static class Program
    {
        public static Server grpcServer { get; private set; }
        public static ManagerClientServices.ManagerClient ManagerClient { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Start the GRPC server with all the services
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // TODO receive this in args
            Program.ManagerClient = new ManagerClientServices.ManagerClient("lm1", EntityType.LeaseManager);

            ServerPort serverPort = new ServerPort("localhost", 9999, ServerCredentials.Insecure);
            Program.grpcServer = new Server
            {
                Services = {
                    ManagerService.BindService(new ManagerClientServices.ServerService(new ManagerClientServices.ServerLogic(Program.ManagerClient))),
                },
                Ports = { serverPort }
            };

            grpcServer.Start();
        }
    }
}