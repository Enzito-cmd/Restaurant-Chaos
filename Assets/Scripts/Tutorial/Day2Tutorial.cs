using System.Collections;
using System.Collections.Generic;
using RestaurantChaos.Clients;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class Day2Tutorial : TutorialSequence
    {
        private enum CookStage
        {
            None,
            Fridge,
            Bread,
            Fry,
            Place,
            Discard
        }

        [Header("Signs")]
        [SerializeField] [TextArea] private string[] unlockedSigns;
        [SerializeField] [TextArea] private string[] kitchenSigns;
        [SerializeField] [TextArea] private string[] phasesSigns;
        [SerializeField] [TextArea] private string[] breadSigns;
        [SerializeField] [TextArea] private string[] frySigns;
        [SerializeField] [TextArea] private string[] placeDishSigns;
        [SerializeField] [TextArea] private string[] completeSigns;

        [Header("Targets")]
        [SerializeField] private Transform fridgeStation;
        [SerializeField] private Transform fridgePoint;
        [SerializeField] private Transform breaderStation;
        [SerializeField] private Transform breaderPoint;
        [SerializeField] private Transform fryerStation;
        [SerializeField] private Transform fryerPoint;
        [SerializeField] private Transform trashBin;

        private readonly HashSet<CookStage> stagesWithSignsShown = new HashSet<CookStage>();

        protected override IEnumerator Run()
        {
            yield return ShowSigns(unlockedSigns);
            yield return KitchenStep(kitchenSigns);
            yield return ShowSigns(phasesSigns);
            yield return CookTempuraStep();
            yield return OpenRestaurantStep();
            yield return ServeClientStep();
            yield return ShowSigns(completeSigns);
        }

        private IEnumerator CookTempuraStep()
        {
            stagesWithSignsShown.Clear();

            CookStage pointedStage = CookStage.None;

            while (FindRestWithPlate() == null)
            {
                if (IsMinigameBusy())
                {
                    yield return null;
                    continue;
                }

                CookStage stage = GetCookStage();

                if (stage != pointedStage)
                {
                    if (!stagesWithSignsShown.Contains(stage))
                    {
                        stagesWithSignsShown.Add(stage);
                        pointer.Clear();
                        pointedStage = CookStage.None;

                        yield return ShowSigns(GetStageSigns(stage));
                        continue;
                    }

                    PointAtStage(stage);
                    pointedStage = stage;
                }
                else if (stage == CookStage.Place)
                {
                    PointAtAvailableRests();
                }

                yield return null;
            }

            pointer.Clear();
        }

        private CookStage GetCookStage()
        {
            if (IsHolding(ItemType.FriedTempura))
            {
                return CookStage.Place;
            }

            if (IsHolding(ItemType.BreadedTempura))
            {
                return CookStage.Fry;
            }

            if (IsHolding(ItemType.Seafood))
            {
                return CookStage.Bread;
            }

            if (holdSystem != null && holdSystem.IsHoldingItem)
            {
                return CookStage.Discard;
            }

            return CookStage.Fridge;
        }

        private string[] GetStageSigns(CookStage stage)
        {
            switch (stage)
            {
                case CookStage.Bread:
                    return breadSigns;
                case CookStage.Fry:
                    return frySigns;
                case CookStage.Place:
                    return placeDishSigns;
                default:
                    return null;
            }
        }

        private void PointAtStage(CookStage stage)
        {
            switch (stage)
            {
                case CookStage.Fridge:
                    pointer.PointAt(ArrowTarget(fridgeStation, fridgePoint), fridgePoint);
                    break;
                case CookStage.Bread:
                    pointer.PointAt(ArrowTarget(breaderStation, breaderPoint), breaderPoint);
                    break;
                case CookStage.Fry:
                    pointer.PointAt(ArrowTarget(fryerStation, fryerPoint), fryerPoint);
                    break;
                case CookStage.Place:
                    PointAtAvailableRests();
                    break;
                case CookStage.Discard:
                    pointer.PointAt(trashBin, false);
                    break;
            }
        }

        private Transform ArrowTarget(Transform station, Transform point)
        {
            if (station != null)
            {
                return station;
            }

            return point;
        }

        private IEnumerator OpenRestaurantStep()
        {
            DayManager.Instance.SetOpeningAllowed(true);

            while (!DayManager.Instance.IsRestaurantOpen)
            {
                yield return null;
            }
        }

        private IEnumerator ServeClientStep()
        {
            HashSet<MoneyPickup> oldMoney = new HashSet<MoneyPickup>(FindObjectsByType<MoneyPickup>(FindObjectsSortMode.None));

            while (ClientRegistry.ActiveCount == 0)
            {
                yield return null;
            }

            ClientBase client = FindFirstObjectByType<ClientBase>();

            while (client != null)
            {
                yield return null;
            }

            MoneyPickup newMoney = FindNewMoney(oldMoney);

            while (newMoney != null)
            {
                yield return null;
            }
        }

        private MoneyPickup FindNewMoney(HashSet<MoneyPickup> oldMoney)
        {
            MoneyPickup[] allMoney = FindObjectsByType<MoneyPickup>(FindObjectsSortMode.None);

            foreach (MoneyPickup money in allMoney)
            {
                if (!oldMoney.Contains(money))
                {
                    return money;
                }
            }

            return null;
        }
    }
}
