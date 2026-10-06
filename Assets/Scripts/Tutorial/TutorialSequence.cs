using System.Collections;
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
            yield return Run();

            runningSequence = null;
            pointer.Clear();

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
