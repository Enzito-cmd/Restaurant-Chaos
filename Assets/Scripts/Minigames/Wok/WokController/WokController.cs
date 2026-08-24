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

    private void Start()
    {
        if (minigameContainer != null)
        {
            minigameContainer.SetActive(false);
        }

        prepPhase = GetComponent<WokPrepPhase>();
        visuals = GetComponent<WokVisuals>();

        if (playerHoldSystem == null)
        {
            playerHoldSystem =
                FindFirstObjectByType<PlayerHoldSystem>();
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

        if (prepPhase != null)
        {
            prepPhase.StartPrepPhase();
        }
    }

    public void EndMinigame()
    {
        if (!isMinigameActive)
            return;

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
        WokTransition transition =
            GetComponent<WokTransition>();

        if (transition != null)
        {
            transition.StartTransition();
        }
    }

    public void FinishCooking(bool won)
    {
        if (won)
        {
            Debug.Log("Wok Won - creando comida");

            if (playerHoldSystem == null)
            {
                playerHoldSystem =
                    FindFirstObjectByType<PlayerHoldSystem>();
            }

            if (playerHoldSystem != null && wokRicePrefab != null)
            {
                // Crea directamente el modelo del wok
                GameObject spawnedFood = Instantiate(wokRicePrefab);

                // Se asegura de que esté activo
                spawnedFood.SetActive(true);

                // Si el modelo no tiene HoldableItem, lo agrega
                HoldableItem holdableItem =
                    spawnedFood.GetComponent<HoldableItem>();

                if (holdableItem == null)
                {
                    holdableItem =
                        spawnedFood.AddComponent<HoldableItem>();
                }

                // Le asigna el tipo correcto
                holdableItem.itemType = ItemType.WokRice;

                // Intenta ponerlo en las manos del jugador
                bool itemGiven =
                    playerHoldSystem.HoldExistingItem(spawnedFood);

                if (itemGiven)
                {
                    Debug.Log("WokRice creado y entregado al jugador.");
                }
                else
                {
                    Debug.LogWarning(
                        "Se creó el WokRice pero el jugador tiene las manos ocupadas."
                    );

                    Destroy(spawnedFood);
                }
            }
            else
            {
                Debug.LogError(
                    "Falta PlayerHoldSystem o el modelo del WokRice."
                );
            }
        }

        if (MinigameManager.Instance != null)
        {
            Debug.Log("Saliendo del minijuego...");
            MinigameManager.Instance.ExitMinigame();
        }
    }
}