using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaseManager.Manager
{
    struct HookConfig
    {
        public bool Enabled;
        public int HookIntervalMs;
    }

    internal class ManagerLogic
    {
        private HookConfig hookConfig;
        private Timer hookTimer;

        public bool StatusHookConfig(bool enabled, int hookIntervalMs)
        {
            lock (this)
            {
                this.hookConfig = new HookConfig { Enabled = enabled, HookIntervalMs = hookIntervalMs };

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

        public void ExecuteHook(object state)
        {
            // TODO call status rpc
        }
    }
}
