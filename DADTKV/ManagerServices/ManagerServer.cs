using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerServices
{
    internal class ManagerServer
    {
        private Timer? hookTimer = null;

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
                    this.hookTimer = new Timer(this.ExecuteHook, null, TimeSpan.FromMilliseconds(hookIntervalMs), TimeSpan.FromMilliseconds(hookIntervalMs));
                else
                    this.hookTimer.Change(TimeSpan.FromMilliseconds(hookIntervalMs), TimeSpan.FromMilliseconds(hookIntervalMs));

                return true;
            }

        }

        private void ExecuteHook(object state)
        {
            // TODO call status rpc
            // Get status of instance and send to hook
        }
    }

}