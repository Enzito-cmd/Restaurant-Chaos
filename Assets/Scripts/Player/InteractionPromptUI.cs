using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private GameObject promptRoot;

    [Header("Interact")]
    [SerializeField] private Image keyIcon;
    [SerializeField] private Sprite interactIcon;
    [SerializeField] private string interactMessage = "Interact";
    [SerializeField] private string interactMessageSpanish = "Interactuar";

    [Header("Extinguisher")]
    [SerializeField] private Sprite extinguisherIcon;
    [SerializeField] private string extinguisherMessage = "Use";
    [SerializeField] private string extinguisherMessageSpanish = "Usar";

    private TMP_Text promptText;
    private bool isShowingExtinguisherMessage;

    private void Start()
    {
        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        if (promptRoot != null)
        {
            promptText = promptRoot.GetComponentInChildren<TMP_Text>(true);

            Refresh();

            promptRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Localization.Changed += Refresh;
    }

    private void OnDisable()
    {
        Localization.Changed -= Refresh;
    }

    private void Update()
    {
        if (playerInteraction == null) return;
        if (promptRoot == null) return;

        bool holdingExtinguisher = playerInteraction.IsHoldingExtinguisher();
        bool canShowPrompt = !Cursor.visible && Time.timeScale > 0f;
        bool shouldShow = canShowPrompt && (holdingExtinguisher || playerInteraction.HasTarget);

        if (shouldShow)
        {
            UpdateMessage(holdingExtinguisher);
        }

        if (promptRoot.activeSelf != shouldShow)
        {
            promptRoot.SetActive(shouldShow);
        }
    }

    private void UpdateMessage(bool useExtinguisherMessage)
    {
        if (useExtinguisherMessage == isShowingExtinguisherMessage)
        {
            return;
        }

        isShowingExtinguisherMessage = useExtinguisherMessage;

        Refresh();
    }

    private void Refresh()
    {
        if (isShowingExtinguisherMessage)
        {
            Show(extinguisherIcon, Localization.Pick(extinguisherMessage, extinguisherMessageSpanish));
        }
        else
        {
            Show(interactIcon, Localization.Pick(interactMessage, interactMessageSpanish));
        }
    }

    private void Show(Sprite icon, string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }

        if (keyIcon != null && icon != null)
        {
            keyIcon.sprite = icon;
        }
    }
}
