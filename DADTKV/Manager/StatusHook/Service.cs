using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.StatusHook
{
    internal class Service : global::ManagerStatusHook.ManagerStatusHookBase
    {
        private ManagerStatusHook statusHook;

        public Service(ManagerStatusHook statusHook)
        {
            this.statusHook = statusHook;
        }

        public override Task<ExecuteResponse> Execute(ExecuteRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ExecuteResponse() { Ok = statusHook.Execute(request.Id, request.Type, request.Status) });
        }

        public void StartServer(string hostname = "localhost", int port = 8000)
        {
            ServerPort serverPort = new ServerPort(hostname, port, ServerCredentials.Insecure);
            Server server = new Server
            {
                Services = { global::ManagerStatusHook.BindService(this) },
                Ports = { serverPort }
            };

            server.Start();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }
    }
}
