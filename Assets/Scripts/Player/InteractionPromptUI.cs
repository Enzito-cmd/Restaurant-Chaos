using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private GameObject promptRoot;

    [Header("Extinguisher")]
    [SerializeField] private string extinguisherMessage = "Press F to use";

    private TMP_Text promptText;
    private string interactMessage;
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

            if (promptText != null)
            {
                interactMessage = promptText.text;
            }

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
        if (promptText == null)
        {
            return;
        }

        if (useExtinguisherMessage == isShowingExtinguisherMessage)
        {
            return;
        }

        isShowingExtinguisherMessage = useExtinguisherMessage;

        if (useExtinguisherMessage)
        {
            promptText.text = extinguisherMessage;
        }
        else
        {
            promptText.text = interactMessage;
        }
    }
}
