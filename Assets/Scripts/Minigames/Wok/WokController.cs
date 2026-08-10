using System.Collections;
using UnityEngine;

public class WokController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject minigameContainer;
    [SerializeField] private WokVisuals visuals;

    private bool isMinigameActive = false;

    private void Start()
    {
        if (minigameContainer != null)
        {
            minigameContainer.SetActive(false);
        }
    }

    public void StartMinigame()
    {
        isMinigameActive = true;

        if (minigameContainer != null)
        {
            minigameContainer.SetActive(true);
        }

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        if (visuals != null)
        {
            yield return StartCoroutine(visuals.AnimatePanIn());
        }
    }

    public void EndMinigame()
    {
        if (!isMinigameActive) return;
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        isMinigameActive = false;

        if (visuals != null)
        {
            yield return StartCoroutine(visuals.AnimatePanOut());
        }

        if (minigameContainer != null)
        {
            minigameContainer.SetActive(false);
        }
    }

    public void FinishCooking(bool won)
    {
        if (won)
        {
            Debug.Log("Wok Won");
            // playerHoldSystem.HoldItem(wokDishPrefab);
        }

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ExitMinigame();
        }
    }
}