using System;
using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Clients
{
    public class ClientPatience : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fillImage;

        public event Action Expired;

        private bool isRunning;
        private float duration;
        private float remaining;

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
            UpdateFill();
        }

        public void BeginSeatedPhase(float durationSeconds, float startFill)
        {
            duration = durationSeconds;
            remaining = durationSeconds * startFill;
            isRunning = true;
            UpdateFill();
        }

        public void Stop()
        {
            isRunning = false;
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

            UpdateFill();
        }

        private void UpdateFill()
        {
            if (fillImage == null) return;

            fillImage.fillAmount = NormalizedFill;
        }
    }
}
