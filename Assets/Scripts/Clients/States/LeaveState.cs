using UnityEngine;
using UnityEngine.AI;

namespace RestaurantChaos.Clients
{
    public class LeaveState : ClientState
    {
        public LeaveState(ClientBase client) : base(client)
        {
        }

        public override void Enter()
        {
            client.Patience.Stop();
            client.OrderDisplay.Clear();

            if (client.CurrentChair != null)
            {
                StandUpFromChair();
            }
            else if (client.Agent != null && !client.Agent.enabled)
            {
                client.Agent.enabled = true;
            }

            if (client.Agent != null && client.Agent.isOnNavMesh)
            {
                client.Agent.stoppingDistance = 0f;
                client.Agent.speed = client.Config.moveSpeed;

                if (client.ExitPoint != null)
                {
                    client.Agent.SetDestination(client.ExitPoint.position);
                }
            }
        }

        private void StandUpFromChair()
        {
            ClientChair chair = client.CurrentChair;
            Vector3 standUpPosition = chair.transform.position + (chair.transform.forward * 0.8f);

            chair.SetOccupied(false);
            client.CurrentChair = null;

            if (NavMesh.SamplePosition(standUpPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                standUpPosition = hit.position;
            }

            if (client.Agent != null)
            {
                client.Agent.enabled = true;
                client.Agent.Warp(standUpPosition);
                client.Agent.isStopped = false;
                client.Agent.ResetPath();
            }
        }

        public override void Tick()
        {
            if (client.ExitPoint == null) return;

            Vector3 clientPos = new Vector3(client.transform.position.x, 0f, client.transform.position.z);
            Vector3 exitPos = new Vector3(client.ExitPoint.position.x, 0f, client.ExitPoint.position.z);

            if (Vector3.Distance(clientPos, exitPos) <= 2.5f)
            {
                client.RemoveFromLevel();
            }
        }

        public override void UpdateAnimation()
        {
            if (client.ExitPoint == null) return;

            bool isWalking = Vector3.Distance(client.transform.position, client.ExitPoint.position) > 0.5f;

            if (isWalking)
            {
                client.Animator.SetFloat("Speed", 1f);
            }
            else
            {
                client.Animator.SetFloat("Speed", 0f);
            }
        }
    }
}
