using ManagerClientServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerClientServices
{
    public class ServerLogic
    {
        private Timer? hookTimer = null;
        private ManagerClient hookClient;

        public ServerLogic(ManagerClient hookClient)
        {
            this.hookClient = hookClient;
        }

        public bool Crash()
        {
            // Suicide the process to emulate a crash
            Environment.Exit(1);
            return true;
        }

        public bool CommunicationDelay(int delayMsPerRequest)
        {
            // TODO: implement
            // Maybe use interceptors (that sleep and block the thread)
            //  and store the delayMsPerRequest in a centralized place
            // Since this is in a library, extending this class can help implement
            //  or use delegates/observers
            return false;
        }

        public bool StatusHookConfig(bool enabled, int hookIntervalMs)
        {
            lock (this)
            {
                if (!enabled)
                {
                    this.hookTimer?.Dispose();
                    this.hookTimer = null;
                    return true;
                }

                if (this.hookTimer == null)
                    this.hookTimer = new Timer(this.hookClient.ExecuteHook!, null, TimeSpan.FromMilliseconds(hookIntervalMs), TimeSpan.FromMilliseconds(hookIntervalMs));
                else
                    this.hookTimer.Change(TimeSpan.FromMilliseconds(hookIntervalMs), TimeSpan.FromMilliseconds(hookIntervalMs));

                return true;
            }
        }
    }

}