namespace RestaurantChaos.Clients
{
    public class ClientStateMachine
    {
        public ClientState Current { get; private set; }

        public void ChangeState(ClientState next)
        {
            if (Current != null)
            {
                Current.Exit();
            }

            Current = next;
            Current.Enter();
        }

        public void Tick()
        {
            if (Current != null)
            {
                Current.Tick();
            }
        }
    }
}
