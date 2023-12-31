﻿using Parser.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Manager.StatusHook
{
    internal class ManagerStatusHookServiceLogic
    {
        public bool Execute(string id, EntityType type, string status)
        {
            // TODO: use this as heartbeat to figure out if a client is dead?

            switch (type)
            {
                case EntityType.Client:
                    lock (Main.Clients)
                    {
                        // Verify it's a valid status
                        if (!ClientStatus.Statuses.Contains(status))
                            return false;

                        Pair<ClientConfigLine, EntityStatus> client = Main.Clients.Find(client => client.First.ID == id)!;
                        if (client == null)
                            return false;

                        client.Second.Status = typeof(ClientStatus).GetPropertyValue<Color>(status);
                        return true;
                    }
                case EntityType.TransactionManager:
                    lock (Main.TransactionManagers)
                    {
                        // Verify it's a valid status
                        if (!TMStatus.Statuses.Contains(status))
                            return false;

                        Pair<ServerConfigLine, EntityStatus> tm = Main.TransactionManagers.Find(tm => tm.First.ID == id)!;
                        if (tm == null)
                            return false;

                        tm.Second.Status = typeof(TMStatus).GetPropertyValue<Color>(status);
                        return true;
                    }
                case EntityType.LeaseManager:
                    lock (Main.LeaseManagers)
                    {
                        // Verify it's a valid status
                        if (!LMStatus.Statuses.Contains(status))
                            return false;

                        Pair<ServerConfigLine, EntityStatus> lm = Main.LeaseManagers.Find(lm => lm.First.ID == id)!;
                        if (lm == null)
                            return false;

                        lm.Second.Status = typeof(LMStatus).GetPropertyValue<Color>(status);
                        return true;
                    }
            }

            return false;
        }
    }

    static class PropertyHelper
    {
        /// <summary>
        /// Gets the value of a static property on a specific type.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="t">The type to search.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this Type t, string name)
        {
            // http://www.java2s.com/Code/CSharp/Reflection/Getsthevalueofastaticpropertyonaspecifictype.htm
            if (t == null)
                return default(T);

            BindingFlags flags = BindingFlags.Static | BindingFlags.Public;

            PropertyInfo info = t.GetProperty(name, flags);

            if (info == null)
            {
                // See if we have a field;
                FieldInfo finfo = t.GetField(name, flags);
                if (finfo == null)
                    return default(T);

                return (T)finfo.GetValue(null);
            }

            return (T)info.GetValue(null, null);
        }
    }
}
