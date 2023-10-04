using Common;
using Grpc.Core;
using Manager.StatusHook;

namespace Manager
{
    internal static class Program
    {
        public static Server GrpcServer { get; private set; }
        public static HostPort ManagerAddress { get; private set; } = new HostPort("localhost", 9999);

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Start the GRPC server with all the services
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            ServerPort serverPort = new ServerPort("0.0.0.0", Program.ManagerAddress.Port, ServerCredentials.Insecure);
            Program.GrpcServer = new Server
            {
                Services = {
                    global::ManagerStatusHook.BindService(new ManagerStatusHookService(new ManagerStatusHookServiceLogic()))
                },
                Ports = { serverPort }
            };

            Program.GrpcServer.Start();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}