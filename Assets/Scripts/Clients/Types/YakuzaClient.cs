namespace RestaurantChaos.Clients
{
    public class YakuzaClient : ClientBase
    {
        protected override void OnPatienceExpired()
        {
            ChangeState(new AngryChaseState(this));
        }
    }
}
