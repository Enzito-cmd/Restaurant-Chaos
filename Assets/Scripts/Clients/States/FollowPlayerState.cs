using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class FollowPlayerState : ClientState
    {
        public FollowPlayerState(ClientBase client) : base(client)
        {
        }

        public override void Enter()
        {
            ClientChair.ShowFreeChairs();
        }

        public override void Tick()
        {
            if (client.Player == null) return;

            float distance = Vector3.Distance(client.transform.position, client.Player.position);

            if (distance > client.StopDistance)
            {
                client.MoveTowards(client.Player.position, client.Config.moveSpeed);
            }
            else if (client.Agent != null)
            {
                client.Agent.ResetPath();
            }
        }

        public override void UpdateAnimation()
        {
            if (client.Player == null) return;

            bool isWalking = Vector3.Distance(client.transform.position, client.Player.position) > client.StopDistance;

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
