using System;
using System.Collections;
using System.Collections.Generic;
using RestaurantChaos.Clients;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class MinigameGuide : MonoBehaviour
    {
        [Serializable]
        public class Step
        {
            [TextArea] public string text;
            public Camera worldCamera;
            public Collider[] colliders;
            public Renderer[] renderers;
            public RectTransform[] uiTargets;
            public Collider[] clickColliders;
            public bool showMouseIcon;
            public GameObject[] showObjects;
            public Color holeTint = new Color(1f, 1f, 1f, 0f);
        }

        [SerializeField] private TutorialSignUI signUI;
        [SerializeField] private Step[] steps;
        [SerializeField] private float highlightPadding = 28f;

        private readonly List<Rect> rects = new List<Rect>();
        private readonly Vector3[] corners = new Vector3[4];
        private int lastPlayedDay = -1;

        public Collider LastClickedCollider { get; private set; }

        public IEnumerator PlayIfNeeded()
        {
            DayManager dayManager = DayManager.Instance;

            if (dayManager == null || !dayManager.IsTutorialDay || dayManager.CurrentDayIndex == lastPlayedDay)
            {
                yield break;
            }

            lastPlayedDay = dayManager.CurrentDayIndex;
            LastClickedCollider = null;

            CursorManager.Request(CursorStyle.Default);

            for (int i = 0; i < steps.Length; i++)
            {
                yield return PlayStep(steps[i], i == steps.Length - 1);
            }
        }

        public Collider ConsumeClickedCollider()
        {
            Collider clicked = LastClickedCollider;
            LastClickedCollider = null;

            return clicked;
        }

        private IEnumerator PlayStep(Step step, bool isLastStep)
        {
            Camera camera = step.worldCamera;

            if (camera == null)
            {
                camera = Camera.main;
            }

            rects.Clear();

            foreach (Collider target in step.colliders)
            {
                AddBounds(target.bounds, camera);
            }

            foreach (Renderer target in step.renderers)
            {
                AddBounds(target.bounds, camera);
            }

            foreach (RectTransform target in step.uiTargets)
            {
                AddUiRect(target);
            }

            SignOptions options = new SignOptions();
            options.highlightRects = new List<Rect>(rects);
            options.holeTint = step.holeTint;
            options.showMouseIcon = step.showMouseIcon;
            options.showObjects = step.showObjects;
            options.releaseAtOnce = isLastStep;

            if (step.clickColliders.Length > 0)
            {
                options.advanceCondition = () => ClickHits(step.clickColliders, camera);
            }

            yield return signUI.ShowPages(new string[] { step.text }, options);
        }

        private bool ClickHits(Collider[] clickColliders, Camera camera)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit) || Array.IndexOf(clickColliders, hit.collider) < 0)
            {
                return false;
            }

            LastClickedCollider = hit.collider;

            return true;
        }

        private void AddBounds(Bounds bounds, Camera camera)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(Sign(i & 1), Sign(i & 2), Sign(i & 4)));
                Vector3 screenPoint = camera.WorldToScreenPoint(corner);

                min = Vector2.Min(min, screenPoint);
                max = Vector2.Max(max, screenPoint);
            }

            AddRect(min, max);
        }

        private void AddUiRect(RectTransform target)
        {
            Camera canvasCamera = target.GetComponentInParent<Canvas>(true).rootCanvas.worldCamera;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            target.GetWorldCorners(corners);

            foreach (Vector3 corner in corners)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, corner);

                min = Vector2.Min(min, screenPoint);
                max = Vector2.Max(max, screenPoint);
            }

            AddRect(min, max);
        }

        private void AddRect(Vector2 min, Vector2 max)
        {
            Vector2 padding = Vector2.one * highlightPadding;

            rects.Add(Rect.MinMaxRect(min.x - padding.x, min.y - padding.y, max.x + padding.x, max.y + padding.y));
        }

        private float Sign(int bit)
        {
            if (bit == 0)
            {
                return -1f;
            }

            return 1f;
        }
    }
}
