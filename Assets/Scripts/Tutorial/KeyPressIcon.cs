using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Tutorial
{
    public class KeyPressIcon : MonoBehaviour
    {
        [SerializeField] private Image keyImage;
        [SerializeField] private float pressPeriod = 1.2f;
        [SerializeField] private float holdTime = 0.4f;
        [SerializeField] private float pressedScale = 0.82f;
        [SerializeField] private float pressedShade = 0.7f;

        private Vector3 baseScale;
        private float press;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            press = 0f;
        }

        private void Update()
        {
            float target = 0f;

            if (Mathf.Repeat(Time.unscaledTime, pressPeriod) < holdTime)
            {
                target = 1f;
            }

            press = Mathf.MoveTowards(press, target, Time.unscaledDeltaTime * 14f);

            float shade = Mathf.Lerp(1f, pressedShade, press);

            transform.localScale = baseScale * Mathf.Lerp(1f, pressedScale, press);
            keyImage.color = new Color(shade, shade, shade, 1f);
        }
    }
}
