using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.StatusHook
{
    internal class ManagerStatusHook
    {
        public ManagerStatusHook(Func<bool> callback)
        {
            this.callback = callback;
        }

        public bool Execute(string id, EntityType type, string status)
        {
            return true;
        }
    }
}
