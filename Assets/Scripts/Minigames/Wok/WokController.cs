using System.Collections;
using UnityEngine;

public class WokController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject minigameContainer;
    
    private WokVisuals visuals;
    private WokPrepPhase prepPhase; 
    private bool isMinigameActive = false;

    private void Start()
    {
        if (minigameContainer != null)
        {
            minigameContainer.SetActive(false);
        }

        prepPhase = GetComponent<WokPrepPhase>();
        visuals = GetComponent<WokVisuals>();
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

        if (prepPhase != null)
        {
            prepPhase.StartPrepPhase();
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