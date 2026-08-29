using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Panel Final")]
    [SerializeField] private GameObject endPanel;

    [Header("Stars")]
    [SerializeField] private GameObject[] stars;

    [Header("Star Animation")]
    [SerializeField] private float delayBetweenStars = 0.5f;
    [SerializeField] private float animationDuration = 0.4f;

    private int clientsServed = 0;
    private bool levelEnded = false;
    private bool hasStartedChecking = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        // Apagar todas las estrellas al comenzar
        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                star.SetActive(false);
            }
        }

        // Esperamos a que aparezcan los clientes
        Invoke(nameof(StartCheckingClients), 1f);
    }

    private void StartCheckingClients()
    {
        hasStartedChecking = true;
    }
    public void RegisterClient()
    {
        // No necesitamos contar clientes vivos.
    }

    private void Update()
    {
        if (!hasStartedChecking || levelEnded)
            return;

        RestaurantClient[] clients =
            FindObjectsByType<RestaurantClient>(
                FindObjectsSortMode.None
            );

        if (clients.Length == 0)
        {
            EndLevel();
        }
    }
    public void AddServedClient()
    {
        clientsServed++;

        Debug.Log(
            "CLIENTE ATENDIDO. Total atendidos: " +
            clientsServed
        );
    }

    private void EndLevel()
    {
        if (levelEnded)
            return;

        levelEnded = true;

        Debug.Log(
            "¡NIVEL TERMINADO! Clientes atendidos: " +
            clientsServed
        );

        ShowEndPanel();
    }

    private void ShowEndPanel()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        // Mostrar cursor
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursor();
        }

        // Empezar animación de estrellas
        StartCoroutine(ShowStars());
    }

    private IEnumerator ShowStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            // Si no atendiste suficientes clientes,
            // dejamos de mostrar estrellas.
            if (i >= clientsServed)
                break;

            if (stars[i] == null)
                continue;

            yield return new WaitForSeconds(delayBetweenStars);

            yield return StartCoroutine(
                AnimateStar(stars[i])
            );
        }
    }

    private IEnumerator AnimateStar(GameObject star)
    {
        star.SetActive(true);

        Transform starTransform = star.transform;

        // Empieza invisible/pequeña
        starTransform.localScale = Vector3.zero;

        float elapsed = 0f;

        // Crece
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / animationDuration;

            // Suavizado
            t = 1f - Mathf.Pow(1f - t, 3f);

            starTransform.localScale =
                Vector3.Lerp(
                    Vector3.zero,
                    Vector3.one,
                    t
                );

            yield return null;
        }

        starTransform.localScale = Vector3.one * 0.8f;

        // Sonido
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(
                SoundType.Stars
            );
        }
    }
}