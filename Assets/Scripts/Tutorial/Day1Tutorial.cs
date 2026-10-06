using System.Collections;
using RestaurantChaos.Clients;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class Day1Tutorial : TutorialSequence
    {
        [Header("Signs")]
        [SerializeField] [TextArea] private string[] welcomeSigns;
        [SerializeField] [TextArea] private string[] kitchenSigns;
        [SerializeField] [TextArea] private string[] cookSigns;
        [SerializeField] [TextArea] private string[] placeDishSigns;
        [SerializeField] [TextArea] private string[] openRestaurantSigns;
        [SerializeField] [TextArea] private string[] seatClientSigns;
        [SerializeField] [TextArea] private string[] orderSigns;
        [SerializeField] [TextArea] private string[] deliverSigns;
        [SerializeField] [TextArea] private string[] collectMoneySigns;
        [SerializeField] [TextArea] private string[] completeSigns;

        [Header("Targets")]
        [SerializeField] private Transform kitchenCheckpoint;
        [SerializeField] private float checkpointRadius = 1.5f;
        [SerializeField] private Transform wokStation;
        [SerializeField] private Transform wokPoint;
        [SerializeField] private Transform doorPoint;

        private PlateRest[] plateRests;
        private ClientBase client;

        private Transform WokArrowTarget
        {
            get
            {
                if (wokStation != null)
                {
                    return wokStation;
                }

                return wokPoint;
            }
        }

        protected override IEnumerator Run()
        {
            plateRests = FindObjectsByType<PlateRest>(FindObjectsSortMode.None);

            yield return ShowSigns(welcomeSigns);
            yield return KitchenStep();
            yield return CookStep();
            yield return PlaceDishStep();
            yield return OpenRestaurantStep();
            yield return SeatClientStep();
            yield return OrderStep();
            yield return DeliverStep();
            yield return CollectMoneyStep();
        }

        private IEnumerator KitchenStep()
        {
            yield return ShowSigns(kitchenSigns);

            pointer.PointAt(kitchenCheckpoint, true);

            while (!IsPlayerNear(kitchenCheckpoint, checkpointRadius))
            {
                yield return null;
            }

            pointer.Clear();
        }

        private IEnumerator CookStep()
        {
            yield return ShowSigns(cookSigns);
            yield return WaitForDishInHand();
        }

        private IEnumerator WaitForDishInHand()
        {
            pointer.PointAt(WokArrowTarget, wokPoint);

            while (!IsHolding(ItemType.WokRice) || IsMinigameBusy())
            {
                yield return null;
            }

            pointer.Clear();
        }

        private IEnumerator PlaceDishStep()
        {
            yield return ShowSigns(placeDishSigns);

            while (FindRestWithPlate() == null)
            {
                if (!IsHolding(ItemType.WokRice))
                {
                    yield return WaitForDishInHand();
                }

                yield return null;
            }
        }

        private IEnumerator OpenRestaurantStep()
        {
            yield return ShowSigns(openRestaurantSigns);

            DayManager.Instance.SetOpeningAllowed(true);
            pointer.PointAt(doorPoint, false);

            while (!DayManager.Instance.IsRestaurantOpen)
            {
                yield return null;
            }

            pointer.Clear();
        }

        private IEnumerator SeatClientStep()
        {
            while (ClientRegistry.ActiveCount == 0)
            {
                yield return null;
            }

            client = FindFirstObjectByType<ClientBase>();

            yield return ShowSigns(seatClientSigns);

            while (client != null && !(client.CurrentState is SitState))
            {
                bool shouldPoint = !client.IsFollowingPlayer;

                if (shouldPoint && !pointer.IsPointing)
                {
                    pointer.PointAt(client.transform, false);
                }
                else if (!shouldPoint && pointer.IsPointing)
                {
                    pointer.Clear();
                }

                yield return null;
            }

            pointer.Clear();
        }

        private IEnumerator OrderStep()
        {
            yield return ShowSigns(orderSigns);

            while (client != null && OrderBoard.OpenTicketCount == 0)
            {
                yield return null;
            }
        }

        private IEnumerator DeliverStep()
        {
            yield return ShowSigns(deliverSigns);

            Transform pointedArrow = null;
            Transform pointedGlow = null;

            while (client != null && !client.IsLeaving)
            {
                PlateRest restWithPlate = FindRestWithPlate();
                Transform wantedArrow = null;
                Transform wantedGlow = null;

                if (restWithPlate != null)
                {
                    wantedArrow = restWithPlate.HeldPlate.transform;
                }
                else if (IsHolding(ItemType.WokRice))
                {
                    wantedArrow = client.transform;
                }
                else
                {
                    wantedArrow = WokArrowTarget;
                    wantedGlow = wokPoint;
                }

                if (wantedArrow != pointedArrow || wantedGlow != pointedGlow)
                {
                    pointedArrow = wantedArrow;
                    pointedGlow = wantedGlow;
                    pointer.PointAt(pointedArrow, pointedGlow);
                }

                yield return null;
            }

            pointer.Clear();
        }

        private IEnumerator CollectMoneyStep()
        {
            yield return ShowSigns(collectMoneySigns);

            MoneyPickup money = FindFirstObjectByType<MoneyPickup>();

            if (money != null)
            {
                pointer.PointAt(money.transform, false);
            }

            while (client != null || money != null)
            {
                yield return null;
            }

            pointer.Clear();

            yield return ShowSigns(completeSigns);
        }

        private PlateRest FindRestWithPlate()
        {
            foreach (PlateRest rest in plateRests)
            {
                if (rest != null && rest.HeldPlate != null)
                {
                    return rest;
                }
            }

            return null;
        }
    }
}
