using UnityEngine;
using UnityEngine.UI;

namespace RestaurantChaos.Tutorial
{
    public class MouseClickIcon : MonoBehaviour
    {
        [SerializeField] private Image baseImage;
        [SerializeField] private Image leftButtonImage;
        [SerializeField] private Color pressedButtonColor = new Color(1f, 0.95f, 0.55f, 0.9f);
        [SerializeField] private float clickPeriod = 1.4f;
        [SerializeField] private float holdTime = 0.45f;
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private float pressedShade = 0.78f;

        private float press;

        private void Update()
        {
            float target = 0f;

            if (Mathf.Repeat(Time.unscaledTime, clickPeriod) < holdTime)
            {
                target = 1f;
            }

            press = Mathf.MoveTowards(press, target, Time.unscaledDeltaTime * 12f);

            float shade = Mathf.Lerp(1f, pressedShade, press);
            Color buttonColor = pressedButtonColor;
            buttonColor.a *= press;

            transform.localScale = Vector3.one * Mathf.Lerp(1f, pressedScale, press);
            baseImage.color = new Color(shade, shade, shade, 1f);
            leftButtonImage.color = buttonColor;
        }
    }
}
