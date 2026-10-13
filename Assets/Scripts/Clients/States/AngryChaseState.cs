using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class AngryChaseState : ClientState
    {
        private const float ArrivalTolerance = 0.15f;
        private const float MovingVelocityThreshold = 0.1f;

        private float lastYellTime = -99f;

        public AngryChaseState(ClientBase client) : base(client)
        {
        }

        public override void Enter()
        {
            SoundManager.Instance?.PlaySound(SoundType.ClientAngry);

            client.NotifyLeftQueue();
            client.OrderDisplay.Clear();

            if (client.CurrentChair != null)
            {
                client.CurrentChair.SetOccupied(false);
                client.CurrentChair = null;
            }

            if (client.Agent != null)
            {
                client.Agent.enabled = true;
                client.Agent.isStopped = false;
            }
        }

        public override void Tick()
        {
            if (client.Player == null) return;
            if (client.Agent == null) return;
            if (!client.Agent.isOnNavMesh) return;

            float distanceToPlayer = Vector3.Distance(client.transform.position, client.Player.position);
            float arrivalDistance = client.Config.chaseStopDistance;
            float yellRange = Mathf.Max(client.Config.yellDistance, arrivalDistance + ArrivalTolerance);

            if (distanceToPlayer <= yellRange && Time.time >= lastYellTime + client.Config.yellCooldown)
            {
                lastYellTime = Time.time;
                client.Agent.isStopped = true;
                client.Agent.ResetPath();

                if (client.Animator != null)
                {
                    client.Animator.SetTrigger("Yell");
                }

                Vector3 direction = (client.Player.position - client.transform.position).normalized;
                direction.y = 0f;

                if (direction != Vector3.zero)
                {
                    client.transform.rotation = Quaternion.LookRotation(direction);
                }

                return;
            }

            if (Time.time < lastYellTime + client.Config.yellDuration)
            {
                client.Agent.isStopped = true;
                return;
            }

            client.Agent.isStopped = false;
            client.MoveTowards(client.Player.position, client.Config.moveSpeed * client.Config.chaseSpeedMultiplier, arrivalDistance);
        }

        public override void UpdateAnimation()
        {
            bool isMoving = false;

            if (client.Agent != null && client.Agent.isOnNavMesh)
            {
                isMoving = client.Agent.velocity.magnitude > MovingVelocityThreshold;
            }

            if (isMoving)
            {
                client.Animator.SetFloat("Speed", 2f);
            }
            else
            {
                client.Animator.SetFloat("Speed", 0f);
            }
        }
    }
}
