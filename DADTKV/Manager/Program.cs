using Grpc.Core;
using Manager.StatusHook;

namespace Manager
{
    internal static class Program
    {
        public static Server grpcServer { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Main());

            // Start the GRPC server with all the services
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            ServerPort serverPort = new ServerPort("localhost", 9999, ServerCredentials.Insecure);
            Program.grpcServer = new Server
            {
                Services = {
                    global::ManagerStatusHook.BindService(new ServerService(new ServerLogic()))
                },
                Ports = { serverPort }
            };

            grpcServer.Start();
        }
    }
}