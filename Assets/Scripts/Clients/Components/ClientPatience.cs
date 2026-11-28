using System;
using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Clients
{
    public class ClientPatience : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image ringFill;

        [Header("Warning")]
        [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.25f;

        public event Action Expired;

        private GameObject ringRoot;
        private Vector3 ringBaseLocalPosition;
        private bool isRunning;
        private bool lowWarned;
        private float duration;
        private float remaining;

        private void Awake()
        {
            if (ringFill != null)
            {
                ringRoot = ringFill.GetComponentInParent<Canvas>(true).gameObject;
                ringBaseLocalPosition = ringRoot.transform.localPosition;
            }

            Hide();
        }

        public float RingWorldY
        {
            get
            {
                if (ringRoot == null)
                {
                    return 0f;
                }

                return ringRoot.transform.position.y;
            }
        }

        public void SetRingWorldY(float worldY)
        {
            if (ringRoot == null)
            {
                return;
            }

            Vector3 position = ringRoot.transform.position;

            position.y = worldY;
            ringRoot.transform.position = position;
        }

        public void ResetRingHeight()
        {
            if (ringRoot != null)
            {
                ringRoot.transform.localPosition = ringBaseLocalPosition;
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
            if (ringRoot != null)
            {
                ringRoot.SetActive(true);
            }
        }

        public void Hide()
        {
            if (ringRoot != null)
            {
                ringRoot.SetActive(false);
            }
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
            if (ringFill != null)
            {
                ringFill.fillAmount = NormalizedFill;
            }
        }
    }
}
