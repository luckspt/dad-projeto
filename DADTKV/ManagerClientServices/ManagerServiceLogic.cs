using ManagerClientServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerClientServices
{
    public delegate bool CommunicationDelay(int delayMsPerRequest);
    public delegate bool StartLeaseManager(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses, int proposerPosition);
    public delegate bool StartTransactionManager(List<string> leaseManagersAddresses, List<string> transactionManagersAddresses);

    public partial class ManagerServiceLogic
    {
        private Timer? hookTimer = null;
        private ManagerClient hookClient;
        public CommunicationDelay? CommunicationDelay { get; set; }
        public StartLeaseManager? StartLeaseManagerDelegate { get; set; }
        public StartTransactionManager? StartTransactionManagerDelegate { get; set; }

        public ManagerServiceLogic(ManagerClient hookClient)
        {
            this.hookClient = hookClient;
            this.StatusHookConfig(true, 1000);
        }

        public bool Crash()
        {
            // Suicide the process to emulate a crash
            Environment.Exit(1);
            return true;
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