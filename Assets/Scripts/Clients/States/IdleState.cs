namespace RestaurantChaos.Clients
{
    public class IdleState : ClientState
    {
        public IdleState(ClientBase client) : base(client)
        {
        }

        public override void Enter()
        {
            if (client.Agent != null)
            {
                client.Agent.ResetPath();
            }

            ClientChair.HideAllIndicators();
        }

        public override void OnInteract()
        {
            if (ClientRegistry.AnyFollowing) return;
            if (client.IsPlayerHoldingItem()) return;

            client.ChangeState(new FollowPlayerState(client));
        }

        public override void UpdateAnimation()
        {
            client.Animator.SetFloat("Speed", 0f);
        }
    }
}
