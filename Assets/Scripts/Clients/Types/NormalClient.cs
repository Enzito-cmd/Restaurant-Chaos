namespace RestaurantChaos.Clients
{
    public class NormalClient : ClientBase
    {
        protected override void OnPatienceExpired()
        {
            ChangeState(new LeaveState(this));
        }
    }
}
