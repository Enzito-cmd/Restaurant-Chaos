using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Cameras")]
    [SerializeField] private GameObject mainCam;
    [SerializeField] private GameObject universalMinigameCam;

    [Header("Config")]
    public float zoomDuration = 1.5f; 

    private GameObject activeTransitionCam;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SwitchToMainCam();
    }

    public void SwitchToMainCam()
    {
        StopAllCoroutines();

        if (mainCam != null) mainCam.SetActive(true);
        if (universalMinigameCam != null) universalMinigameCam.SetActive(false);
        if (activeTransitionCam != null) activeTransitionCam.SetActive(false);

        activeTransitionCam = null;
    }

    /// <summary>
    /// MinigameManager call this function when a minigame is triggered.
    /// </summary>
    public void StartMinigameTransition(GameObject transitionCam)
    {
        activeTransitionCam = transitionCam;
        StartCoroutine(MinigameTransitionRoutine());
    }

    private IEnumerator MinigameTransitionRoutine()
    {
        if (mainCam != null) mainCam.SetActive(false);
        if (activeTransitionCam != null) activeTransitionCam.SetActive(true);

        yield return new WaitForSeconds(zoomDuration);

        if (activeTransitionCam != null) activeTransitionCam.SetActive(false);
        if (universalMinigameCam != null) universalMinigameCam.SetActive(true);
    }
    public void StartMinigameExitTransition()
    {
        StartCoroutine(MinigameExitRoutine());
    }

    private IEnumerator MinigameExitRoutine()
    {
        if (universalMinigameCam != null) universalMinigameCam.SetActive(false);
        if (activeTransitionCam != null) activeTransitionCam.SetActive(true);

        yield return null;

        if (activeTransitionCam != null) activeTransitionCam.SetActive(false);
        if (mainCam != null) mainCam.SetActive(true);
    }
}