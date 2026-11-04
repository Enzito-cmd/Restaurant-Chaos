using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RestaurantChaos.Clients
{
    public class ClientPatience : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image ringFill;
        [SerializeField, FormerlySerializedAs("fillImage")] private Image barFill;

        [Header("Warning")]
        [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.25f;

        public event Action Expired;

        private GameObject ringRoot;
        private GameObject barRoot;
        private bool isRunning;
        private bool lowWarned;
        private float duration;
        private float remaining;

        private void Awake()
        {
            ringRoot = GetRoot(ringFill);
            barRoot = GetRoot(barFill);

            if (barRoot != null)
            {
                Billboard.AttachAlwaysVisible(barRoot);
            }

            Hide();
        }

        private static GameObject GetRoot(Image image)
        {
            if (image == null)
            {
                return null;
            }

            return image.GetComponentInParent<Canvas>(true).gameObject;
        }

        private static void SetVisible(GameObject root, bool visible)
        {
            if (root != null)
            {
                root.SetActive(visible);
            }
        }

        public float NormalizedFill
        {
            get
            {
                if (duration <= 0f)
                {
                    return 0f;
                }

                return remaining / duration;
            }
        }

        public void BeginQueuePhase(float durationSeconds)
        {
            duration = durationSeconds;
            remaining = durationSeconds;
            isRunning = true;
            lowWarned = false;
            ShowRing();
            UpdateFill();
        }

        public void BeginSeatedPhase(float durationSeconds, float startFill)
        {
            duration = durationSeconds;
            remaining = durationSeconds * startFill;
            isRunning = true;
            lowWarned = false;
            UpdateFill();
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void ShowRing()
        {
            SetVisible(ringRoot, true);
            SetVisible(barRoot, false);
        }

        public void ShowBar()
        {
            SetVisible(ringRoot, false);
            SetVisible(barRoot, true);
        }

        public void Hide()
        {
            SetVisible(ringRoot, false);
            SetVisible(barRoot, false);
        }

        private void Update()
        {
            if (!isRunning) return;

            remaining -= Time.deltaTime;

            if (remaining <= 0f)
            {
                remaining = 0f;
                isRunning = false;
                UpdateFill();

                if (Expired != null)
                {
                    Expired.Invoke();
                }

                return;
            }

            if (!lowWarned && NormalizedFill <= lowThreshold)
            {
                lowWarned = true;
                SoundManager.Instance?.PlaySound(SoundType.PatienceLow);
            }

            UpdateFill();
        }

        private void UpdateFill()
        {
            float fill = NormalizedFill;

            if (ringFill != null)
            {
                ringFill.fillAmount = fill;
            }

            if (barFill != null)
            {
                barFill.fillAmount = fill;
            }
        }
    }
}
