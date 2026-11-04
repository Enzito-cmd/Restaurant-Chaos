using RestaurantChaos.Tutorial;
using UnityEngine;

public class ExitHintUI : MonoBehaviour
{
    [SerializeField] private GameObject hintRoot;

    private void Update()
    {
        MinigameManager manager = MinigameManager.Instance;

        bool show = manager != null && manager.isMinigameActive && !manager.isTransitioning && !manager.ExitLocked && !TutorialSignUI.IsAnySignShowing && !WorldTime.IsMenuPaused;

        if (hintRoot.activeSelf != show)
        {
            hintRoot.SetActive(show);
        }
    }
}
