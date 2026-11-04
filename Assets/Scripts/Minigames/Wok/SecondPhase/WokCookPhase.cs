using System.Collections;
using UnityEngine;
using TMPro;

public class WokCookPhase : MonoBehaviour
{
    private const float RecenterDistance = 0.1f;

    [Header("References")]
    [SerializeField] private WokController wokController;

    [Header("Wok Control")]
    public Transform wokTransform;
    public Transform wokCenterHitbox;
    public Camera wokCamera;
    public Collider dragPlaneCollider;

    [Header("Wok Grab Settings")]
    public Collider handleCollider;
    public float maxDragDistance = 2.5f;
    public float returnSpeed = 8f;

    [Header("Wok Tilt Settings")]
    public float maxTiltAngle = 25f;
    public float tiltSpeed = 12f;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 startPos;
    private Quaternion startRot;

    [Header("Arrow System")]
    public GameObject arrowsCanvas;
    public GameObject movingArrowPrefab;
    public Transform[] fixedArrowPoints;
    public float spawnInterval = 1.5f;
    public int maxArrowsOnScreen = 3;

    [Header("Toss Skillcheck")]
    [SerializeField] private WokTossPhase tossPhase;
    [SerializeField] private float tossDistance = 1.5f;

    [Header("Cursor")]
    [SerializeField] private CursorStyle idleCursor = CursorStyle.OpenHand;
    [SerializeField] private CursorStyle grabCursor = CursorStyle.ClosedHand;

    [Header("Game Rules")]
    public int maxErrors = 3;
    public int requiredHits = 10;

    [Header("UI Text")]
    public TextMeshProUGUI hitsText;
    public TextMeshProUGUI errorsText;

    private int currentErrors = 0;
    private int currentHits = 0;
    private int activeArrowsCount = 0;
    private float spawnTimer = 0f;

    private Plane dragPlane;
    private bool isCookingActive = false;
    private bool isTossPhase = false;
    private bool isTossing = false;
    private bool isRecentering = false;
    private Vector3 screenUp;
    private Coroutine tossRoutine;

    private Color[] arrowColors = new Color[]
    {
        Color.blue,
        Color.red,
        Color.green,
        new Color(0.5f, 0f, 0.5f),
        new Color(1f, 0.4f, 0.7f)
    };

    private void Awake()
    {
        if (wokController == null)
        {
            wokController = GetComponent<WokController>();
        }


        if (hitsText != null) hitsText.gameObject.SetActive(false);
        if (errorsText != null) errorsText.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        SoundManager.Instance?.StopLoopingSound(SoundType.StoveLoop);
        ResetWokTransform();
    }

    public void ResetWokTransform()
    {
        isDragging = false;
        if (wokTransform != null)
        {
            wokTransform.position = startPos;
            wokTransform.rotation = startRot;
        }
    }

    public void ShowArrowSlots()
    {
        if (arrowsCanvas != null)
        {
            arrowsCanvas.SetActive(true);
        }
    }

    public void StartCooking()
    {
        if (wokTransform != null)
        {
            startPos = wokTransform.position;
            startRot = wokTransform.rotation;
        }

        if (arrowsCanvas != null)
        {
            arrowsCanvas.SetActive(true);
        }

        if (hitsText != null) hitsText.gameObject.SetActive(true);
        if (errorsText != null) errorsText.gameObject.SetActive(true);

        currentErrors = 0;
        currentHits = 0;
        activeArrowsCount = 0;
        spawnTimer = spawnInterval;

        if (dragPlaneCollider != null)
        {
            dragPlane = new Plane(Vector3.up, dragPlaneCollider.transform.position);
        }

        isTossPhase = false;
        isTossing = false;
        isRecentering = false;
        isCookingActive = true;
        SoundManager.Instance?.StartLoopingSound(SoundType.StoveLoop);
        UpdateUI();

        Debug.Log("Cooking started");
    }

    private void Update()
    {
        if (!isCookingActive) return;

        HandleWokDragging();
        HandleWokTilt();
        UpdateCursor();

        if (isTossPhase)
        {
            UpdateToss();
        }
        else
        {
            HandleArrowSpawning();
        }
    }

    private void UpdateCursor()
    {
        if (isDragging)
        {
            CursorManager.Request(grabCursor);
        }
        else
        {
            CursorManager.Request(idleCursor);
        }
    }

    private void HandleWokDragging()
    {
        if (Input.GetMouseButtonDown(0) && !isTossing && !isRecentering)
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider == handleCollider)
                {
                    isDragging = true;

                    if (dragPlane.Raycast(ray, out float enter))
                    {
                        dragOffset = wokTransform.position - ray.GetPoint(enter);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);

            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 desiredPos = ray.GetPoint(enter) + dragOffset;
                Vector3 offsetFromStart = desiredPos - startPos;
                offsetFromStart.y = 0;

                if (offsetFromStart.magnitude > maxDragDistance)
                {
                    desiredPos = startPos + offsetFromStart.normalized * maxDragDistance;
                }

                wokTransform.position = Vector3.Lerp(wokTransform.position, desiredPos, Time.deltaTime * 15f);
            }
        }
        else
        {
            wokTransform.position = Vector3.Lerp(wokTransform.position, startPos, Time.deltaTime * returnSpeed);
        }
    }

    private void HandleWokTilt()
    {
        Vector3 offset = wokTransform.position - startPos;
        offset.y = 0;

        if (offset.magnitude > 0.01f)
        {
            float tiltRatio = Mathf.Clamp01(offset.magnitude / maxDragDistance);
            Vector3 dirToCenter = -offset.normalized;
            Vector3 tiltAxis = Vector3.Cross(Vector3.up, dirToCenter);

            Quaternion tiltRotation = Quaternion.AngleAxis(maxTiltAngle * tiltRatio, tiltAxis);
            Quaternion targetRotation = tiltRotation * startRot;

            wokTransform.rotation = Quaternion.Lerp(wokTransform.rotation, targetRotation, Time.deltaTime * tiltSpeed);
        }
        else
        {
            wokTransform.rotation = Quaternion.Lerp(wokTransform.rotation, startRot, Time.deltaTime * tiltSpeed);
        }
    }

    private void HandleArrowSpawning()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && activeArrowsCount < maxArrowsOnScreen)
        {
            SpawnRandomArrow();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnRandomArrow()
    {
        if (fixedArrowPoints == null || fixedArrowPoints.Length == 0) return;

        int randomIndex = Random.Range(0, fixedArrowPoints.Length);
        Transform targetPoint = fixedArrowPoints[randomIndex];

        Vector3 directionFromCenter = (targetPoint.position - targetPoint.parent.position).normalized;
        Vector3 spawnPos = targetPoint.position + directionFromCenter * 5f;

        GameObject newArrow = Instantiate(movingArrowPrefab, spawnPos, targetPoint.rotation, arrowsCanvas.transform);
        newArrow.transform.localScale = Vector3.one;

        UnityEngine.UI.Image arrowImage = newArrow.GetComponent<UnityEngine.UI.Image>();

        if (arrowImage != null)
        {
            arrowImage.color = arrowColors[Random.Range(0, arrowColors.Length)];
        }

        CookingArrow arrowScript = newArrow.GetComponent<CookingArrow>();

        if (arrowScript == null)
        {
            Destroy(newArrow);
            return;
        }

        float randomSpeed = Random.Range(2f, 4f);
        arrowScript.Initialize(targetPoint.position, randomSpeed, wokCenterHitbox);

        arrowScript.onHit = OnArrowHit;
        arrowScript.onMiss = OnArrowMiss;

        activeArrowsCount++;
    }

    private void OnArrowHit()
    {
        activeArrowsCount--;

        if (!isCookingActive) return;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.WokHit);
        }
        currentHits++;
        UpdateUI();
        Debug.Log(currentHits + "/" + requiredHits);

        if (currentHits >= requiredHits)
        {
            BeginToss();
        }
    }

    private void OnArrowMiss()
    {
        activeArrowsCount--;

        if (!isCookingActive) return;
        SoundManager.Instance?.PlaySound(SoundType.CookingMiss);

        currentErrors++;
        UpdateUI();
        Debug.Log(currentErrors + "/" + maxErrors);

        if (currentErrors >= maxErrors)
        {
            LoseMinigame();
        }
    }

    private void UpdateUI()
    {
        if (hitsText != null)
        {
            hitsText.text = $"{currentHits}/{requiredHits}";
        }

        if (errorsText != null)
        {
            errorsText.text = $"{currentErrors}/{maxErrors}";
        }
    }

    private void BeginToss()
    {
        if (isTossPhase) return;

        if (tossPhase == null)
        {
            WinMinigame();
            return;
        }

        isTossPhase = true;
        isRecentering = true;
        isDragging = false;
        screenUp = Vector3.ProjectOnPlane(wokCamera.transform.up, Vector3.up).normalized;

        HideCookUI();
        tossPhase.Begin();
    }

    private void UpdateToss()
    {
        if (isTossing) return;

        if (isRecentering)
        {
            if ((wokTransform.position - startPos).sqrMagnitude <= RecenterDistance * RecenterDistance)
            {
                isRecentering = false;
            }

            return;
        }

        float reach = Mathf.Min(tossDistance, maxDragDistance * 0.95f);
        float progress = Mathf.Clamp01(Vector3.Dot(wokTransform.position - startPos, screenUp) / reach);

        tossPhase.SetProgress(progress);

        if (progress >= 1f)
        {
            isTossing = true;
            isDragging = false;
            tossRoutine = StartCoroutine(TossRoutine());
        }
    }

    private IEnumerator TossRoutine()
    {
        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ExitLocked = true;
        }

        yield return tossPhase.Play();

        tossRoutine = null;
        WinMinigame();
    }

    private void WinMinigame()
    {
        if (!isCookingActive) return;

        StopCooking();

        Debug.Log("Wok won");

        if (wokController != null)
        {
            wokController.FinishCooking(true);
        }
    }

    private void LoseMinigame()
    {
        if (!isCookingActive) return;

        StopCooking();

        Debug.Log("Wok lost");

        if (wokController != null)
        {
            wokController.FinishCooking(false);
        }
    }

    public void StopCooking()
    {
        if (!isCookingActive) return;

        isCookingActive = false;
        SoundManager.Instance?.StopLoopingSound(SoundType.StoveLoop);
        isTossPhase = false;
        isTossing = false;
        isRecentering = false;
        ResetWokTransform();
        HideCookUI();

        if (tossRoutine != null)
        {
            StopCoroutine(tossRoutine);
            tossRoutine = null;
        }

        if (tossPhase != null)
        {
            tossPhase.ResetState();
        }

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ExitLocked = false;
        }
    }

    private void HideCookUI()
    {
        ClearArrows();

        if (arrowsCanvas != null)
        {
            arrowsCanvas.SetActive(false);
        }

        if (hitsText != null) hitsText.gameObject.SetActive(false);
        if (errorsText != null) errorsText.gameObject.SetActive(false);
    }

    private void ClearArrows()
    {
        if (arrowsCanvas == null) return;

        CookingArrow[] remainingArrows = arrowsCanvas.GetComponentsInChildren<CookingArrow>(true);

        foreach (CookingArrow arrow in remainingArrows)
        {
            Destroy(arrow.gameObject);
        }

        activeArrowsCount = 0;
    }
}