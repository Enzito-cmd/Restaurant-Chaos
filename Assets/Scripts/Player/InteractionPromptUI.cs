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

    [Header("Extinguisher")]
    [SerializeField] private Sprite extinguisherIcon;
    [SerializeField] private string extinguisherMessage = "Use";

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

            Show(interactIcon, interactMessage);

            promptRoot.SetActive(false);
        }
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

        if (useExtinguisherMessage)
        {
            Show(extinguisherIcon, extinguisherMessage);
        }
        else
        {
            Show(interactIcon, interactMessage);
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
