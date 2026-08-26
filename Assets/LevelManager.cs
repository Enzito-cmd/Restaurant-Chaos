using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Panel Final")]
    [SerializeField] private GameObject endPanel;

    [Header("Stars")]
    [SerializeField] private GameObject[] stars;

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

        foreach (GameObject star in stars)
        {
            if (star != null)
            {
                star.SetActive(false);
            }
        }

        // Esperamos un momento para que aparezcan los clientes
        Invoke(nameof(StartCheckingClients), 1f);
    }

    private void StartCheckingClients()
    {
        hasStartedChecking = true;
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

    public void RegisterClient()
    {
        // Ya no necesitamos contar clientes vivos.
    }

    public void AddServedClient()
    {
        clientsServed++;

        Debug.Log(
            "CLIENTE ATENDIDO. Total atendidos: " +
            clientsServed
        );
    }

    public void RemoveClient(bool wasServed)
    {
        // Ya no contamos estrellas acá.
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

        // Mostrar cursor para poder usar los botones
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursor();
        }

        // Mostrar las estrellas correspondientes
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(i < clientsServed);
            }
        }
    }
}