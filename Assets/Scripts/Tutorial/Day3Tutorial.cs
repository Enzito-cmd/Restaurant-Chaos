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

        [Header("Signs (Spanish)")]
        [SerializeField] [TextArea] private string[] welcomeSignsEs;
        [SerializeField] [TextArea] private string[] openRestaurantSignsEs;
        [SerializeField] [TextArea] private string[] yakuzaSignsEs;
        [SerializeField] [TextArea] private string[] angrySignsEs;
        [SerializeField] [TextArea] private string[] completeSignsEs;

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

        public override bool AllowsInteraction(IInteractable interactable)
        {
            return !(interactable is WokStation || interactable is TempuraStation || interactable is FryerStation || interactable is MochiStation || interactable is Fridge);
        }

        protected override IEnumerator Run()
        {
            FindExtinguisher();

            yield return ShowSigns(welcomeSigns, welcomeSignsEs);
            yield return OpenRestaurantStep();
            yield return MeetYakuzaStep();
            yield return WaitForAngryStep();
            yield return DefeatYakuzaStep();
            yield return ShowSigns(completeSigns, completeSignsEs);
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
            yield return ShowSigns(openRestaurantSigns, openRestaurantSignsEs);
            yield return WaitForRestaurantOpen();
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
                yield return ShowSigns(yakuzaSigns, yakuzaSignsEs);
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

            yield return ShowSigns(angrySigns, angrySignsEs);

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
