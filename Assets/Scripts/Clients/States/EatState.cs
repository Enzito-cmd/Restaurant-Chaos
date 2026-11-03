using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class EatState : ClientState
    {
        private float remaining;

        public EatState(ClientBase client) : base(client)
        {
        }

        public override void Enter()
        {
            remaining = client.EatDuration;

            client.Patience.Stop();
            client.Patience.Hide();
            client.OrderDisplay.Clear();
        }

        public override void Tick()
        {
            remaining -= Time.deltaTime;

            if (remaining <= 0f)
            {
                client.ChangeState(new LeaveState(client));
            }
        }

        public override void UpdateAnimation()
        {
            client.Animator.SetBool("Sitting", true);
            client.Animator.SetFloat("Speed", 0f);
        }
    }
}
