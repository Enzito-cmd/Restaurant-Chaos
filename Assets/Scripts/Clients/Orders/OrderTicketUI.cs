using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Clients
{
    public class OrderTicketUI : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image icon;

        private void Start()
        {
            Hide();
        }

        public void Show(MealDefinition meal)
        {
            if (meal == null) return;

            if (icon != null)
            {
                icon.sprite = meal.uiIcon;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
