using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class FollowPlayerState : ClientState
    {
        private const float WalkingVelocityThreshold = 0.1f;

        public FollowPlayerState(ClientBase client) : base(client)
        {
        }

        public override void Enter()
        {
            ClientChair.ShowFreeChairs();
        }

        public override void OnInteract()
        {
            client.ChangeState(new IdleState(client));
        }

        public override void Tick()
        {
            if (client.Player == null)
            {
                return;
            }

            client.ApproachTarget(client.Player.position, client.Config.moveSpeed, client.FollowDistance);
        }

        public override void UpdateAnimation()
        {
            bool isWalking = false;

            if (client.Agent != null && client.Agent.isOnNavMesh)
            {
                isWalking = client.Agent.velocity.magnitude > WalkingVelocityThreshold;
            }

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
