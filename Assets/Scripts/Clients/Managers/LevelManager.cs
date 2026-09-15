using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RestaurantChaos.Clients
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private ClientSpawner spawner;

        [Header("End Panel")]
        [SerializeField] private GameObject endPanel;

        [Header("Stars")]
        [SerializeField] private GameObject[] stars;
        [SerializeField] private float delayBetweenStars = 0.5f;
        [SerializeField] private float starAnimationDuration = 0.4f;

        private int clientsServed;
        private bool levelEnded;
        private bool hasStartedChecking;

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
            ResetForNewDay();
        }

        public void StartNewDay()
        {
            ResetForNewDay();
        }

        private void ResetForNewDay()
        {
            levelEnded = false;
            hasStartedChecking = false;
            clientsServed = 0;

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

            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.HideCursor();
            }

            CancelInvoke(nameof(StartCheckingClients));
            Invoke(nameof(StartCheckingClients), 1f);
        }

        private void OnEnable()
        {
            ClientRegistry.ClientServed += HandleClientServed;
        }

        private void OnDisable()
        {
            ClientRegistry.ClientServed -= HandleClientServed;
        }

        private void StartCheckingClients()
        {
            hasStartedChecking = true;
        }

        private void Update()
        {
            if (!hasStartedChecking) return;
            if (levelEnded) return;
            if (spawner != null && !spawner.HasFinishedSpawning) return;
            if (ClientRegistry.ActiveCount > 0) return;

            EndLevel();
        }

        private void HandleClientServed(ClientBase client)
        {
            clientsServed++;
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("Menu");
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
            Vector3 targetScale = starTransform.localScale;
            starTransform.localScale = Vector3.zero;

            float elapsed = 0f;

            while (elapsed < starAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / starAnimationDuration;
                t = 1f - Mathf.Pow(1f - t, 3f);

                starTransform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                yield return null;
            }

            starTransform.localScale = targetScale * 0.8f;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(SoundType.Stars);
            }
        }
    }
}
