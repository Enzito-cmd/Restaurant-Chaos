using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Clients
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private ClientSpawner spawner;

        [Header("End Panel")]
        [SerializeField] private UIPanelPop endPanel;

        [Header("Stars")]
        [SerializeField] private GameObject[] stars;
        [SerializeField] private float delayBetweenStars = 0.5f;
        [SerializeField] private float starAnimationDuration = 0.4f;

        [Header("Rating (sprites: 0-1 stars, 2 stars, 3 stars)")]
        [SerializeField] private Image ratingImage;
        [SerializeField] private Sprite[] ratingSprites;

        private int clientsServed;
        private bool levelEnded;
        private bool hasStartedChecking;
        private bool endPanelEnabled = true;

        public bool LevelEnded => levelEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (endPanel != null)
            {
                endPanel.HideInstantly();
            }
        }

        private void Start()
        {
            ResetForNewDay();
        }

        public void StartNewDay(bool showEndPanel = true)
        {
            endPanelEnabled = showEndPanel;
            ResetForNewDay();
        }

        private void ResetForNewDay()
        {
            levelEnded = false;
            hasStartedChecking = false;
            clientsServed = 0;

            if (endPanel != null)
            {
                endPanel.Hide();
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
            if (!endPanelEnabled) return;
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
            SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

            SceneTransition.Load("Menu");
        }

        private void EndLevel()
        {
            if (levelEnded) return;

            levelEnded = true;
            ShowEndPanel();
        }

        private void ShowEndPanel()
        {
            HideResults();

            if (endPanel != null)
            {
                endPanel.Show();
            }

            if (CursorManager.Instance != null)
            {
                CursorManager.Instance.ShowCursor();
            }

            StartCoroutine(ShowStars());
        }

        private void HideResults()
        {
            foreach (GameObject star in stars)
            {
                if (star != null)
                {
                    star.SetActive(false);
                }
            }

            if (ratingImage != null)
            {
                ratingImage.gameObject.SetActive(false);
            }
        }

        private IEnumerator ShowStars()
        {
            int earned = Mathf.Min(clientsServed, stars.Length);

            for (int i = 0; i < earned; i++)
            {
                yield return new WaitForSeconds(delayBetweenStars);
                if (!levelEnded) yield break;

                SoundManager.Instance?.PlaySound(SoundType.Stars);
                yield return Pop(stars[i].transform);
            }

            if (ratingImage == null || ratingSprites.Length == 0) yield break;

            yield return new WaitForSeconds(delayBetweenStars);
            if (!levelEnded) yield break;

            ratingImage.sprite = ratingSprites[Mathf.Clamp(earned - 1, 0, ratingSprites.Length - 1)];
            ratingImage.SetNativeSize();
            yield return Pop(ratingImage.transform);
        }

        private IEnumerator Pop(Transform target)
        {
            Vector3 finalScale = target.localScale;
            target.localScale = Vector3.zero;
            target.gameObject.SetActive(true);

            for (float t = 0f; t < 1f; t += Time.deltaTime / starAnimationDuration)
            {
                target.localScale = finalScale * UIPanelPop.EaseOutBack(t);
                yield return null;
            }

            target.localScale = finalScale;
        }
    }
}
