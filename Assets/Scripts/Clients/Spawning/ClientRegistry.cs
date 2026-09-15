using System;
using System.Collections.Generic;

namespace RestaurantChaos.Clients
{
    public static class ClientRegistry
    {
        private static readonly List<ClientBase> activeClients = new List<ClientBase>();
        private static readonly HashSet<ClientBase> servedClients = new HashSet<ClientBase>();

        public static event Action<ClientBase> ClientSpawned;
        public static event Action<ClientBase> ClientServed;
        public static event Action<ClientBase> ClientLost;
        public static event Action<ClientBase> ClientDespawned;

        public static int ActiveCount => activeClients.Count;

        public static bool AnyChasing
        {
            get
            {
                foreach (ClientBase client in activeClients)
                {
                    if (client.IsChasing)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool AnyFollowing
        {
            get
            {
                foreach (ClientBase client in activeClients)
                {
                    if (client.IsFollowingPlayer)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static ClientBase GetFollowingClient()
        {
            foreach (ClientBase client in activeClients)
            {
                if (client.IsFollowingPlayer)
                {
                    return client;
                }
            }

            return null;
        }

        public static void Register(ClientBase client)
        {
            if (activeClients.Contains(client)) return;

            activeClients.Add(client);
            client.Served += () => HandleServed(client);
            client.Despawned += () => HandleDespawned(client);

            if (ClientSpawned != null)
            {
                ClientSpawned.Invoke(client);
            }
        }

        private static void HandleServed(ClientBase client)
        {
            servedClients.Add(client);

            if (ClientServed != null)
            {
                ClientServed.Invoke(client);
            }
        }

        private static void HandleDespawned(ClientBase client)
        {
            bool wasServed = servedClients.Contains(client);

            activeClients.Remove(client);
            servedClients.Remove(client);

            if (!wasServed)
            {
                if (ClientLost != null)
                {
                    ClientLost.Invoke(client);
                }
            }

            if (ClientDespawned != null)
            {
                ClientDespawned.Invoke(client);
            }
        }

        public static void ClearAll()
        {
            activeClients.Clear();
            servedClients.Clear();
        }
    }
}
