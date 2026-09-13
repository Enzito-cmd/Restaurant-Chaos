namespace RestaurantChaos.Clients
{
    public abstract class ClientState
    {
        protected readonly ClientBase client;

        protected ClientState(ClientBase client)
        {
            this.client = client;
        }

        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void Exit() { }
        public virtual void OnInteract() { }
        public virtual void UpdateAnimation() { }
    }
}
