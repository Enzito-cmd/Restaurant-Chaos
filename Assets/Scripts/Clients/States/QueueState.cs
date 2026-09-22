using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class QueueState : ClientState
    {
        private Transform queuePosition;

        public QueueState(ClientBase client, Transform queuePosition) : base(client)
        {
            this.queuePosition = queuePosition;
        }

        public void SetQueuePosition(Transform newPosition)
        {
            queuePosition = newPosition;
        }

        public override void Enter()
        {
            client.Patience.BeginQueuePhase(client.Config.queuePatienceSeconds);
        }

        public override void Tick()
        {
            if (queuePosition == null) return;

            float distance = Vector3.Distance(client.transform.position, queuePosition.position);

            if (distance > client.StopDistance)
            {
                client.MoveTowards(queuePosition.position, client.Config.moveSpeed);
            }
            else if (client.Agent != null && client.Agent.hasPath)
            {
                client.Agent.ResetPath();
            }
        }

        public override void OnInteract()
        {
            if (!client.IsFrontOfQueue) return;
            if (ClientRegistry.AnyFollowing) return;
            if (client.IsPlayerHoldingItem()) return;

            SoundManager.Instance?.PlaySound(SoundType.ClientPicked);
            client.ChangeState(new FollowPlayerState(client));
        }

        public override void Exit()
        {
            client.NotifyLeftQueue();
        }

        public override void UpdateAnimation()
        {
            if (queuePosition == null) return;

            bool isWalking = Vector3.Distance(client.transform.position, queuePosition.position) > client.StopDistance;

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
