using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private GameObject promptRoot;

    private void Start()
    {
        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInteraction == null) return;
        if (promptRoot == null) return;

        bool shouldShow = playerInteraction.HasTarget && !Cursor.visible && Time.timeScale > 0f;

        if (promptRoot.activeSelf != shouldShow)
        {
            promptRoot.SetActive(shouldShow);
        }
    }
}
