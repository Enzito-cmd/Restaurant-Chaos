using System.Collections;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class SitState : ClientState
    {
        private readonly ClientChair chair;
        private float queueEndFill;

        public SitState(ClientBase client, ClientChair chair) : base(client)
        {
            this.chair = chair;
        }

        public override void Enter()
        {
            queueEndFill = client.Patience.NormalizedFill;
            client.Patience.Stop();

            client.CurrentChair = chair;
            chair.SetOccupied(true);
            ClientChair.HideAllIndicators();

            SoundManager.Instance?.PlaySound(SoundType.ClientSit);

            if (client.Agent != null)
            {
                client.Agent.isStopped = true;
                client.Agent.enabled = false;
            }

            if (chair.SitPoint != null)
            {
                client.transform.position = chair.SitPoint.position;
                client.transform.rotation = chair.SitPoint.rotation;
            }

            client.ChosenMeal = client.Config.ChooseMeal();

            client.StartCoroutine(OrderRoutine());
        }

        private IEnumerator OrderRoutine()
        {
            client.OrderDisplay.Show(client.ThinkingPrefab);

            yield return new WaitForSeconds(client.ThinkingDuration);

            if (client.ChosenMeal != null)
            {
                client.OrderDisplay.Show(client.ChosenMeal.orderVisualPrefab);
                OrderBoard.OpenTicket(chair.TableNumber, client.ChosenMeal, client);
                SoundManager.Instance?.PlaySound(SoundType.pedido);
            }

            float startFill = client.Config.GetSeatedStartFill(queueEndFill);
            client.Patience.BeginSeatedPhase(client.Config.seatedPatienceSeconds, startFill);
        }

        public override void Exit()
        {
            OrderBoard.CloseTicket(chair.TableNumber);
        }

        public override void OnInteract()
        {
            client.TryDeliverFood();
        }

        public override void UpdateAnimation()
        {
            client.Animator.SetBool("Sitting", true);
            client.Animator.SetFloat("Speed", 0f);
        }
    }
}
