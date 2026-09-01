using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ClientQueueSpawner spawner;

    [Header("Panel Final")]
    [SerializeField] private GameObject endPanel;

    [Header("Stars")]
    [SerializeField] private GameObject[] stars;

    [Header("Star Animation")]
    [SerializeField] private float delayBetweenStars = 0.5f;
    [SerializeField] private float animationDuration = 0.4f;

    [Header("Level Settings")]
    public bool isLevel2 = false; 

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

        Invoke(nameof(StartCheckingClients), 1f);
    }

    private void StartCheckingClients()
    {
        hasStartedChecking = true;
    }

    private void Update()
    {
        if (!hasStartedChecking || levelEnded) return;

        if (spawner != null && !spawner.HasFinishedSpawning) return;

        RestaurantClient[] clients = FindObjectsByType<RestaurantClient>(FindObjectsSortMode.None);

        if (clients.Length == 0)
        {
            EndLevel();
        }
    }

    public void AddServedClient()
    {
        clientsServed++;
    }

    private void EndLevel()
    {
        if (levelEnded) return;

        levelEnded = true;
        ShowEndPanel();
    }

    private void ShowEndPanel()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursor();
        }

        StartCoroutine(ShowStars());
    }

    private IEnumerator ShowStars()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            if (i >= clientsServed) break;

            if (stars[i] == null) continue;

            yield return new WaitForSeconds(delayBetweenStars);
            yield return StartCoroutine(AnimateStar(stars[i]));
        }
    }

    private IEnumerator AnimateStar(GameObject star)
    {
        star.SetActive(true);
        Transform starTransform = star.transform;
        starTransform.localScale = Vector3.zero;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            starTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        starTransform.localScale = Vector3.one * 0.8f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.Stars);
        }
    }
}