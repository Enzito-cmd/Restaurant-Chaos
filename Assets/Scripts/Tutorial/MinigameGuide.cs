using System.Collections;
using System.Collections.Generic;
using RestaurantChaos.Clients;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class MinigameGuide : MonoBehaviour
    {
        [System.Serializable]
        public class Step
        {
            [TextArea] public string text;
            public Camera worldCamera;
            public Transform[] worldTargets;
            public RectTransform[] uiTargets;
        }

        [Header("Steps")]
        [SerializeField] private Step[] steps;

        [Header("Highlight")]
        [SerializeField] private float highlightPadding = 28f;

        private readonly List<Rect> stepRects = new List<Rect>();
        private readonly List<Renderer> renderersBuffer = new List<Renderer>();
        private readonly Vector3[] cornersBuffer = new Vector3[4];
        private TutorialSignUI signUI;
        private int lastPlayedDayIndex = -1;

        public IEnumerator PlayIfNeeded()
        {
            if (!ShouldPlay())
            {
                yield break;
            }

            signUI = FindFirstObjectByType<TutorialSignUI>();

            if (signUI == null)
            {
                yield break;
            }

            lastPlayedDayIndex = DayManager.Instance.CurrentDayIndex;

            CursorManager.Request(CursorStyle.Default);

            foreach (Step step in steps)
            {
                if (string.IsNullOrWhiteSpace(step.text))
                {
                    continue;
                }

                BuildRects(step);

                yield return signUI.ShowPages(new string[] { step.text }, stepRects);
            }

            while (signUI != null && signUI.IsShowing)
            {
                yield return null;
            }
        }

        private bool ShouldPlay()
        {
            if (steps == null || steps.Length == 0)
            {
                return false;
            }

            DayManager dayManager = DayManager.Instance;

            if (dayManager == null || !dayManager.IsTutorialDay)
            {
                return false;
            }

            return lastPlayedDayIndex != dayManager.CurrentDayIndex;
        }

        private void BuildRects(Step step)
        {
            stepRects.Clear();

            if (step.worldTargets != null)
            {
                Camera camera = step.worldCamera;

                if (camera == null)
                {
                    camera = Camera.main;
                }

                foreach (Transform target in step.worldTargets)
                {
                    if (target != null && camera != null && TryGetWorldRect(target, camera, out Rect worldRect))
                    {
                        stepRects.Add(Inflate(worldRect));
                    }
                }
            }

            if (step.uiTargets != null)
            {
                foreach (RectTransform target in step.uiTargets)
                {
                    if (target != null && TryGetUiRect(target, out Rect uiRect))
                    {
                        stepRects.Add(Inflate(uiRect));
                    }
                }
            }
        }

        private Rect Inflate(Rect rect)
        {
            rect.xMin -= highlightPadding;
            rect.xMax += highlightPadding;
            rect.yMin -= highlightPadding;
            rect.yMax += highlightPadding;

            return rect;
        }

        private bool TryGetWorldRect(Transform target, Camera camera, out Rect rect)
        {
            rect = new Rect();

            if (!TryGetWorldBounds(target, out Bounds bounds))
            {
                return false;
            }

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector2 screenMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 screenMax = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3(
                    PickEdge(i & 1, min.x, max.x),
                    PickEdge(i & 2, min.y, max.y),
                    PickEdge(i & 4, min.z, max.z));

                Vector3 screenPoint = camera.WorldToScreenPoint(corner);

                if (screenPoint.z <= 0f)
                {
                    return false;
                }

                screenMin = Vector2.Min(screenMin, screenPoint);
                screenMax = Vector2.Max(screenMax, screenPoint);
            }

            rect = Rect.MinMaxRect(screenMin.x, screenMin.y, screenMax.x, screenMax.y);

            return true;
        }

        private float PickEdge(int bit, float low, float high)
        {
            if (bit == 0)
            {
                return low;
            }

            return high;
        }

        private bool TryGetWorldBounds(Transform target, out Bounds bounds)
        {
            bounds = new Bounds(target.position, Vector3.zero);

            bool found = false;

            target.GetComponentsInChildren(false, renderersBuffer);

            foreach (Renderer targetRenderer in renderersBuffer)
            {
                if (!targetRenderer.enabled || targetRenderer is ParticleSystemRenderer || targetRenderer is TrailRenderer)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = targetRenderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(targetRenderer.bounds);
                }
            }

            if (found)
            {
                return true;
            }

            Collider targetCollider = target.GetComponentInChildren<Collider>();

            if (targetCollider != null)
            {
                bounds = targetCollider.bounds;
                return true;
            }

            return false;
        }

        private bool TryGetUiRect(RectTransform target, out Rect rect)
        {
            rect = new Rect();

            Canvas canvas = target.GetComponentInParent<Canvas>();
            Camera canvasCamera = null;

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvasCamera = canvas.worldCamera;
            }

            target.GetWorldCorners(cornersBuffer);

            Vector2 screenMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 screenMax = new Vector2(float.MinValue, float.MinValue);

            foreach (Vector3 corner in cornersBuffer)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, corner);

                screenMin = Vector2.Min(screenMin, screenPoint);
                screenMax = Vector2.Max(screenMax, screenPoint);
            }

            rect = Rect.MinMaxRect(screenMin.x, screenMin.y, screenMax.x, screenMax.y);

            return true;
        }
    }
}
