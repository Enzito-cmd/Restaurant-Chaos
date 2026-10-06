using UnityEngine;

namespace RestaurantChaos.Tutorial
{
    public class TutorialPointer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private GameObject floorGlowPrefab;

        [Header("Arrow")]
        [SerializeField] private float arrowScale = 3f;
        [SerializeField] private float arrowHeight = 2.5f;
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private float bobAmplitude = 0.2f;
        [SerializeField] private float bobSpeed = 3f;

        [Header("Floor Glow")]
        [SerializeField] private float glowHeightOffset = 0.02f;

        private Transform arrowTarget;
        private Transform glowTarget;
        private Transform arrowAnchor;
        private GameObject glowInstance;

        public bool IsPointing => arrowTarget != null || glowTarget != null;

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

            arrowTarget = newArrowTarget;
            glowTarget = newGlowTarget;

            if (arrowTarget != null && arrowPrefab != null)
            {
                arrowAnchor = new GameObject("TutorialArrow").transform;
                arrowAnchor.localScale = Vector3.one * arrowScale;

                GameObject arrow = Instantiate(arrowPrefab, arrowAnchor);

                arrow.transform.localPosition = Vector3.zero;

                DisableColliders(arrowAnchor.gameObject);
            }

            if (glowTarget != null && floorGlowPrefab != null)
            {
                glowInstance = Instantiate(floorGlowPrefab, glowTarget.position, floorGlowPrefab.transform.rotation);
                DisableColliders(glowInstance);
            }

            UpdatePositions();
        }

        private void DisableColliders(GameObject root)
        {
            foreach (Collider markerCollider in root.GetComponentsInChildren<Collider>())
            {
                markerCollider.enabled = false;
            }
        }

        public void Clear()
        {
            arrowTarget = null;
            glowTarget = null;

            if (arrowAnchor != null)
            {
                Destroy(arrowAnchor.gameObject);
                arrowAnchor = null;
            }

            if (glowInstance != null)
            {
                Destroy(glowInstance);
                glowInstance = null;
            }
        }

        private void LateUpdate()
        {
            if (arrowAnchor != null && arrowTarget == null)
            {
                Destroy(arrowAnchor.gameObject);
                arrowAnchor = null;
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
            if (arrowAnchor != null && arrowTarget != null)
            {
                float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

                arrowAnchor.position = arrowTarget.position + Vector3.up * (arrowHeight + bob);
                arrowAnchor.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            }

            if (glowInstance != null && glowTarget != null)
            {
                glowInstance.transform.position = glowTarget.position + Vector3.up * glowHeightOffset;
            }
        }
    }
}
