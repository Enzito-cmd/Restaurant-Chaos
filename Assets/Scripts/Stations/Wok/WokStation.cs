using UnityEngine;

public class WokStation : MonoBehaviour, IInteractable, IMinigame
{
    [Header("Camera")]
    [SerializeField] private GameObject transitionCam;

    [Header("References")]
    [SerializeField] private PlayerHoldSystem playerHoldSystem;
    [SerializeField] private WokController wokController;

    public void Interact()
    {
        if (MinigameManager.Instance != null && (MinigameManager.Instance.isMinigameActive || MinigameManager.Instance.isTransitioning))
        {
            return;
        }

        if (playerHoldSystem != null && playerHoldSystem.IsHoldingItem)
        {
            Debug.Log("Hands must be empty to use the Wok");
            return;
        }

        StartMinigame();
    }

    private void StartMinigame()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.EnterMinigame(transitionCam, this);
        }
    }

    public void SetupMinigame()
    {
        if (wokController != null) wokController.StartMinigame();
    }

    public void EndMinigame()
    {
        if (wokController != null) wokController.EndMinigame();
    }
}