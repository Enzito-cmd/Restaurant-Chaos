using System.Collections;
using RestaurantChaos.Clients;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class Day3Tutorial : TutorialSequence
    {
        [Header("Signs")]
        [SerializeField] [TextArea] private string[] welcomeSigns;
        [SerializeField] [TextArea] private string[] openRestaurantSigns;
        [SerializeField] [TextArea] private string[] yakuzaSigns;
        [SerializeField] [TextArea] private string[] angrySigns;
        [SerializeField] [TextArea] private string[] completeSigns;

        [Header("Targets")]
        [SerializeField] private Transform extinguisherStation;
        [SerializeField] private Transform extinguisherPoint;

        private ClientBase yakuza;

        private Transform ExtinguisherArrowTarget
        {
            get
            {
                if (extinguisherStation != null)
                {
                    return extinguisherStation;
                }

                return extinguisherPoint;
            }
        }

        protected override IEnumerator Run()
        {
            FindExtinguisher();

            yield return ShowSigns(welcomeSigns);
            yield return OpenRestaurantStep();
            yield return MeetYakuzaStep();
            yield return WaitForAngryStep();
            yield return DefeatYakuzaStep();
            yield return ShowSigns(completeSigns);
        }

        private void FindExtinguisher()
        {
            if (extinguisherStation != null)
            {
                return;
            }

            ExtinguisherProp extinguisher = FindFirstObjectByType<ExtinguisherProp>(FindObjectsInactive.Include);

            if (extinguisher != null)
            {
                extinguisherStation = extinguisher.transform;
            }
        }

        private IEnumerator OpenRestaurantStep()
        {
            yield return ShowSigns(openRestaurantSigns);

            DayManager.Instance.SetOpeningAllowed(true);

            while (!DayManager.Instance.IsRestaurantOpen)
            {
                yield return null;
            }
        }

        private IEnumerator MeetYakuzaStep()
        {
            while (ClientRegistry.ActiveCount == 0)
            {
                yield return null;
            }

            yakuza = FindFirstObjectByType<ClientBase>();

            yield return WaitForClientToBeSeen(yakuza);

            if (yakuza != null)
            {
                yield return ShowSigns(yakuzaSigns);
            }
        }

        private IEnumerator WaitForAngryStep()
        {
            while (yakuza != null && !yakuza.IsChasing)
            {
                yield return null;
            }
        }

        private IEnumerator DefeatYakuzaStep()
        {
            if (yakuza == null)
            {
                yield break;
            }

            yield return ShowSigns(angrySigns);

            bool isPointing = false;

            while (yakuza != null)
            {
                bool shouldPoint = !playerInteraction.IsHoldingExtinguisher();

                if (shouldPoint && !isPointing)
                {
                    pointer.PointAt(ExtinguisherArrowTarget, extinguisherPoint);
                    isPointing = true;
                }
                else if (!shouldPoint && isPointing)
                {
                    pointer.Clear();
                    isPointing = false;
                }

                yield return null;
            }

            pointer.Clear();
        }
    }
}
