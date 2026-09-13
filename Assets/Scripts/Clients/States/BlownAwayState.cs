using System.Collections;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class BlownAwayState : ClientState
    {
        private const float destroyDelay = 2.5f;
        private const float torqueStrength = 10f;

        private readonly Vector3 force;

        public BlownAwayState(ClientBase client, Vector3 force) : base(client)
        {
            this.force = force;
        }

        public override void Enter()
        {
            client.Patience.Stop();
            client.OrderDisplay.Clear();

            if (client.CurrentChair != null)
            {
                client.CurrentChair.SetOccupied(false);
                client.CurrentChair = null;
            }

            if (client.Agent != null)
            {
                client.Agent.isStopped = true;
                client.Agent.enabled = false;
            }

            Rigidbody rb = client.GetComponent<Rigidbody>();

            if (rb == null)
            {
                rb = client.gameObject.AddComponent<Rigidbody>();
            }

            rb.isKinematic = false;
            rb.useGravity = true;

            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * torqueStrength, ForceMode.Impulse);

            client.StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(destroyDelay);
            client.RemoveFromLevel();
        }
    }
}
