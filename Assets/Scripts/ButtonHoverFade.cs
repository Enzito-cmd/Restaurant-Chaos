using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverFade : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup hoverGroup;
    [SerializeField] private float fadeSpeed = 8f;

    private float target;

    private void OnEnable()
    {
        target = 0f;
        hoverGroup.alpha = 0f;
    }

    private void Update()
    {
        hoverGroup.alpha = Mathf.MoveTowards(hoverGroup.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target = 0f;
    }
}
