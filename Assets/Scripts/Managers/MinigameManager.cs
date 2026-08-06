using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [Header("State")]
    public bool isMinigameActive = false;
    public bool isTransitioning = false;

    [Header("References")]
    [SerializeField] private PlayerController player;

    private IMinigame currentMinigame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (isMinigameActive && !isTransitioning && Input.GetKeyDown(KeyCode.Q))
        {
            ExitMinigame();
        }
    }

    /// <summary>
    /// Stations calls this function when triggered.
    /// </summary>
    public void EnterMinigame(GameObject transitionCam, IMinigame minigame)
    {
        if (isTransitioning || isMinigameActive) return;

        StartCoroutine(EnterMinigameRoutine(transitionCam, minigame));
    }

    private System.Collections.IEnumerator EnterMinigameRoutine(GameObject transitionCam, IMinigame minigame)
    {
        isTransitioning = true; 
        isMinigameActive = true;
        currentMinigame = minigame;

        if (player != null) player.SetMovement(false);
        if (CursorManager.Instance != null) CursorManager.Instance.ShowCursor();

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.StartMinigameTransition(transitionCam);
            yield return new WaitForSeconds(CameraManager.Instance.zoomDuration);
        }
        if (currentMinigame != null) currentMinigame.SetupMinigame();

        isTransitioning = false; 
    }

    /// <summary>
    /// This functions ends the minigame.
    /// </summary>
    public void ExitMinigame()
    {
        if (isTransitioning) return; 
        StartCoroutine(ExitMinigameRoutine());
    }

    private System.Collections.IEnumerator ExitMinigameRoutine()
    {
        isTransitioning = true; 

        if (currentMinigame != null)
        {
            currentMinigame.EndMinigame();
            currentMinigame = null;
        }

        if (CursorManager.Instance != null) CursorManager.Instance.HideCursor();

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.StartMinigameExitTransition();
            yield return new WaitForSeconds(CameraManager.Instance.zoomDuration);
        }

        if (player != null) player.SetMovement(true);

        isMinigameActive = false;
        isTransitioning = false;
    }
}