using System.Collections;
using UnityEngine;

namespace RestaurantChaos.Clients
{
    public class ScoreEnvelope : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform ticket;

        [Header("Envelope")]
        [SerializeField] private float riseDuration = 0.55f;
        [SerializeField] private float pauseBeforeTicket = 0.15f;

        [Header("Ticket")]
        [SerializeField] private float ticketHiddenOffset = 660f;
        [SerializeField] private float ticketDuration = 0.9f;

        private RectTransform frame;
        private Vector2 framePosition;
        private Vector2 ticketPosition;
        private bool captured;

        public void Prepare()
        {
            if (!captured)
            {
                frame = (RectTransform)transform;
                framePosition = frame.anchoredPosition;
                ticketPosition = ticket.anchoredPosition;
                captured = true;
            }

            frame.anchoredPosition = framePosition + Vector2.down * frame.rect.height;
            ticket.anchoredPosition = ticketPosition + Vector2.down * ticketHiddenOffset;
        }

        public IEnumerator Play()
        {
            yield return Slide(frame, framePosition, riseDuration, true);
            yield return new WaitForSecondsRealtime(pauseBeforeTicket);
            yield return Slide(ticket, ticketPosition, ticketDuration, false);
        }

        private static IEnumerator Slide(RectTransform target, Vector2 to, float duration, bool brakeAtEnd)
        {
            Vector2 from = target.anchoredPosition;

            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                float eased = Mathf.SmoothStep(0f, 1f, t);

                if (brakeAtEnd)
                {
                    eased = 1f - (1f - t) * (1f - t) * (1f - t);
                }

                target.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);

                yield return null;
            }

            target.anchoredPosition = to;
        }
    }
}
