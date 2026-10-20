using System.Collections;
using System.Collections.Generic;
using RestaurantChaos.Clients;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public abstract class TutorialSequence : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected TutorialSignUI signUI;
        [SerializeField] protected TutorialPointer pointer;
        [SerializeField] protected PlayerController player;
        [SerializeField] protected PlayerInteraction playerInteraction;
        [SerializeField] protected PlayerHoldSystem holdSystem;

        [Header("Kitchen")]
        [SerializeField] protected Transform kitchenCheckpoint;
        [SerializeField] protected float checkpointRadius = 1.5f;

        [Header("Clients")]
        [SerializeField] protected float clientSightDelay = 2.5f;

        protected PlateRest[] plateRests;

        private readonly List<Transform> availableRestPoints = new List<Transform>();
        private Coroutine runningSequence;

        public void Begin()
        {
            if (runningSequence != null)
            {
                StopCoroutine(runningSequence);
            }

            runningSequence = StartCoroutine(RunAndFinish());
        }

        protected abstract IEnumerator Run();

        private IEnumerator RunAndFinish()
        {
            plateRests = FindObjectsByType<PlateRest>(FindObjectsSortMode.None);

            yield return Run();

            runningSequence = null;
            pointer.Clear();

            while (signUI.IsShowing)
            {
                yield return null;
            }

            DayManager.Instance.StartNextDay();
        }

        protected IEnumerator ShowSigns(string[] pages)
        {
            if (pages == null || pages.Length == 0)
            {
                yield break;
            }

            while (IsMinigameBusy())
            {
                yield return null;
            }

            SetPlayerFrozen(true);
            yield return signUI.ShowPages(pages);
            SetPlayerFrozen(false);
        }

        protected IEnumerator WaitForClientToBeSeen(ClientBase client)
        {
            float elapsed = 0f;

            while (client != null && elapsed < clientSightDelay && !(client.CurrentState is SitState))
            {
                elapsed += Time.deltaTime;

                yield return null;
            }
        }

        protected IEnumerator KitchenStep(string[] signs)
        {
            yield return ShowSigns(signs);
            yield return GoToKitchen();
        }

        protected IEnumerator GoToKitchen()
        {
            pointer.PointAt(kitchenCheckpoint, true);

            while (!IsPlayerNear(kitchenCheckpoint, checkpointRadius))
            {
                yield return null;
            }

            pointer.Clear();
        }

        protected PlateRest FindRestWithPlate()
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

        protected void PointAtAvailableRests()
        {
            availableRestPoints.Clear();

            foreach (PlateRest rest in plateRests)
            {
                if (rest != null && rest.CanDepositHeldPlate())
                {
                    availableRestPoints.Add(rest.PlatePoint);
                }
            }

            pointer.PointAtAll(availableRestPoints);
        }

        private void SetPlayerFrozen(bool frozen)
        {
            player.SetMovement(!frozen);
            playerInteraction.SetLocked(frozen);
        }

        protected bool IsMinigameBusy()
        {
            return MinigameManager.Instance != null && MinigameManager.Instance.IsBusy;
        }

        protected bool IsPlayerNear(Transform point, float radius)
        {
            Vector3 offset = player.transform.position - point.position;
            offset.y = 0f;

            return offset.magnitude <= radius;
        }

        protected bool IsHolding(ItemType itemType)
        {
            if (holdSystem == null || !holdSystem.IsHoldingItem)
            {
                return false;
            }

            HoldableItem item = holdSystem.GetHeldItem().GetComponentInChildren<HoldableItem>();

            return item != null && item.itemType == itemType;
        }
    }
}
