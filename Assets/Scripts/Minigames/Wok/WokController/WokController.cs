using System.Collections;
using UnityEngine;

public class WokController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject minigameContainer;

    [Header("Food")]
    [SerializeField] private GameObject wokRicePrefab;
    [SerializeField] private PlayerHoldSystem playerHoldSystem;

    private WokVisuals visuals;
    private WokPrepPhase prepPhase;
    private bool isMinigameActive = false;
    private WokTransition transition;

    private void Start()
    {
        if (minigameContainer != null)
        {
            minigameContainer.SetActive(false);
        }

        prepPhase = GetComponent<WokPrepPhase>();
        visuals = GetComponent<WokVisuals>();
        transition = GetComponent<WokTransition>();
    }

    public void StartMinigame()
    {
        SoundManager.Instance?.PlaySound(SoundType.WokStart);
        isMinigameActive = true;

        // REACTIVAR HUEVOS Y ARROZ
        if (transition != null)
        {
            transition.ResetBowls();
        }

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

    public void CookPhase()
    {
        WokTransition transition = GetComponent<WokTransition>();

        if (transition != null)
        {
            transition.StartTransition();
        }
    }

    public void FinishCooking(bool won)
    {
        if (won)
        {
            if (playerHoldSystem != null && wokRicePrefab != null)
            {
                GameObject spawnedFood = Instantiate(wokRicePrefab);
                spawnedFood.SetActive(true);

                bool itemGiven = playerHoldSystem.HoldExistingItem(spawnedFood);

                if (itemGiven)
                {
                    Debug.Log("Wok handled");
                }
                else
                {
                    Destroy(spawnedFood);
                }
            }
        }

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ExitMinigame();
        }
    }
}