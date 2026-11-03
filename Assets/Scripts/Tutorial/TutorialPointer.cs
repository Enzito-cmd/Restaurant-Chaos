using System.Collections.Generic;
using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class TutorialPointer : MonoBehaviour
    {
        private class ArrowMarker
        {
            public Transform target;
            public Transform anchor;
            public bool fitsToBody;
            public bool hasHeight;
            public float currentHeight;
        }

        [Header("Prefabs")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private GameObject floorGlowPrefab;

        [Header("Arrow")]
        [SerializeField] private float arrowScale = 3f;
        [SerializeField] private float arrowHeight = 2.5f;
        [SerializeField] private float topClearance = 0.8f;
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private float bobAmplitude = 0.2f;
        [SerializeField] private float bobSpeed = 3f;

        [Header("Character Arrow")]
        [SerializeField] private float bodyClearance = 1f;
        [SerializeField] private float heightFollowSpeed = 6f;

        [Header("Floor Glow")]
        [SerializeField] private float glowHeightOffset = 0.02f;

        private readonly List<ArrowMarker> arrows = new List<ArrowMarker>();
        private readonly List<SkinnedMeshRenderer> bodyRenderers = new List<SkinnedMeshRenderer>();
        private Transform glowTarget;
        private GameObject glowInstance;

        public bool IsPointing => arrows.Count > 0 || glowTarget != null;

        public void PointAt(Transform target, bool showFloorGlow)
        {
            Transform glow = null;

            if (showFloorGlow)
            {
                glow = target;
            }

            PointAt(target, glow);
        }

        public void PointAt(Transform newArrowTarget, Transform newGlowTarget)
        {
            Clear();

            if (newArrowTarget == null && newGlowTarget == null)
            {
                return;
            }

            if (newArrowTarget != null)
            {
                AddArrow(newArrowTarget, false);
            }

            glowTarget = newGlowTarget;

            if (glowTarget != null && floorGlowPrefab != null)
            {
                glowInstance = Instantiate(floorGlowPrefab, glowTarget.position, floorGlowPrefab.transform.rotation);
                DisableColliders(glowInstance);
            }

            UpdatePositions();
        }

        public void PointAtCharacter(Transform character)
        {
            Clear();

            if (character == null)
            {
                return;
            }

            AddArrow(character, true);
            UpdatePositions();
        }

        public void PointAtAll(List<Transform> targets)
        {
            if (glowTarget == null && HasSameArrowTargets(targets))
            {
                return;
            }

            Clear();

            foreach (Transform target in targets)
            {
                if (target != null)
                {
                    AddArrow(target, false);
                }
            }

            UpdatePositions();
        }

        public void Clear()
        {
            foreach (ArrowMarker marker in arrows)
            {
                if (marker.anchor != null)
                {
                    Destroy(marker.anchor.gameObject);
                }
            }

            arrows.Clear();
            glowTarget = null;

            if (glowInstance != null)
            {
                Destroy(glowInstance);
                glowInstance = null;
            }
        }

        private void AddArrow(Transform target, bool fitsToBody)
        {
            ArrowMarker marker = new ArrowMarker();
            marker.target = target;
            marker.fitsToBody = fitsToBody;

            if (arrowPrefab != null)
            {
                marker.anchor = new GameObject("TutorialArrow").transform;
                marker.anchor.localScale = Vector3.one * arrowScale;

                GameObject arrow = Instantiate(arrowPrefab, marker.anchor);

                arrow.transform.localPosition = Vector3.zero;

                DisableColliders(marker.anchor.gameObject);
            }

            arrows.Add(marker);
        }

        private bool HasSameArrowTargets(List<Transform> targets)
        {
            if (arrows.Count != targets.Count)
            {
                return false;
            }

            for (int i = 0; i < arrows.Count; i++)
            {
                if (arrows[i].target != targets[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void DisableColliders(GameObject root)
        {
            foreach (Collider markerCollider in root.GetComponentsInChildren<Collider>())
            {
                markerCollider.enabled = false;
            }
        }

        private void LateUpdate()
        {
            for (int i = arrows.Count - 1; i >= 0; i--)
            {
                if (arrows[i].target == null)
                {
                    if (arrows[i].anchor != null)
                    {
                        Destroy(arrows[i].anchor.gameObject);
                    }

                    arrows.RemoveAt(i);
                }
            }

            if (glowInstance != null && glowTarget == null)
            {
                Destroy(glowInstance);
                glowInstance = null;
            }

            UpdatePositions();
        }

        private void UpdatePositions()
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

            foreach (ArrowMarker marker in arrows)
            {
                if (marker.anchor == null)
                {
                    continue;
                }

                UpdateHeight(marker);

                marker.anchor.position = GetAimPosition(marker.target) + Vector3.up * (marker.currentHeight + bob);
                marker.anchor.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            }

            if (glowInstance != null && glowTarget != null)
            {
                glowInstance.transform.position = glowTarget.position + Vector3.up * glowHeightOffset;
            }
        }

        private Vector3 GetAimPosition(Transform target)
        {
            Vector3 position = target.position;

            if (target.TryGetComponent(out Collider targetCollider) && targetCollider.enabled)
            {
                Vector3 center = targetCollider.bounds.center;

                position.x = center.x;
                position.z = center.z;
            }

            return position;
        }

        private bool TryGetColliderTop(Transform target, out float top)
        {
            top = 0f;

            if (!target.TryGetComponent(out Collider targetCollider) || !targetCollider.enabled)
            {
                return false;
            }

            top = targetCollider.bounds.max.y;

            return true;
        }

        private void UpdateHeight(ArrowMarker marker)
        {
            float wantedHeight = arrowHeight;

            if (marker.fitsToBody)
            {
                float bodyTop = GetBodyTopOffset(marker.target);

                wantedHeight = Mathf.Max(arrowHeight, bodyTop + bodyClearance);
            }
            else if (TryGetColliderTop(marker.target, out float colliderTop))
            {
                wantedHeight = Mathf.Max(arrowHeight, colliderTop + topClearance - marker.target.position.y);
            }

            if (!marker.hasHeight)
            {
                marker.currentHeight = wantedHeight;
                marker.hasHeight = true;
                return;
            }

            marker.currentHeight = Mathf.MoveTowards(marker.currentHeight, wantedHeight, heightFollowSpeed * Time.deltaTime);
        }

        private float GetBodyTopOffset(Transform target)
        {
            target.GetComponentsInChildren(bodyRenderers);

            bool foundBody = false;
            float top = 0f;

            foreach (SkinnedMeshRenderer bodyRenderer in bodyRenderers)
            {
                if (!bodyRenderer.enabled)
                {
                    continue;
                }

                if (!foundBody || bodyRenderer.bounds.max.y > top)
                {
                    top = bodyRenderer.bounds.max.y;
                    foundBody = true;
                }
            }

            if (!foundBody)
            {
                return 0f;
            }

            return top - target.position.y;
        }
    }
}
