using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class ClientOrderDisplay : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Transform orderVisualPoint;

        private GameObject currentVisual;

        public void Show(GameObject prefab)
        {
            Clear();

            if (prefab == null) return;
            if (orderVisualPoint == null) return;

            currentVisual = Instantiate(prefab, orderVisualPoint.position, orderVisualPoint.rotation, orderVisualPoint);
            currentVisual.transform.localRotation = prefab.transform.rotation;

            Vector3 prefabScale = prefab.transform.localScale;
            Vector3 parentScale = orderVisualPoint.lossyScale;
            currentVisual.transform.localScale = new Vector3(
                prefabScale.x / parentScale.x,
                prefabScale.y / parentScale.y,
                prefabScale.z / parentScale.z);
        }

        public void Clear()
        {
            if (currentVisual == null) return;

            Destroy(currentVisual);
            currentVisual = null;
        }
    }
}
