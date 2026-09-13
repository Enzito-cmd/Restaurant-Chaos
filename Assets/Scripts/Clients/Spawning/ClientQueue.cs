using System.Collections.Generic;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class ClientQueue : MonoBehaviour
    {
        [Header("Queue Positions")]
        [SerializeField] private Transform[] queuePositions;

        private readonly List<ClientBase> queuedClients = new List<ClientBase>();

        public int Capacity => queuePositions.Length;
        public bool HasSpace => queuedClients.Count < queuePositions.Length;

        public void Enqueue(ClientBase client)
        {
            if (!HasSpace) return;

            queuedClients.Add(client);
            client.LeftQueue += () => HandleClientLeftQueue(client);

            int index = queuedClients.Count - 1;
            client.EnterQueue(queuePositions[index]);

            RefreshPositions();
        }

        private void HandleClientLeftQueue(ClientBase client)
        {
            queuedClients.Remove(client);
            RefreshPositions();
        }

        private void RefreshPositions()
        {
            for (int i = 0; i < queuedClients.Count; i++)
            {
                ClientBase client = queuedClients[i];

                client.IsFrontOfQueue = (i == 0);
                client.UpdateQueuePosition(queuePositions[i]);
            }
        }
    }
}
