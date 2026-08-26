using UnityEngine;
using TMPro;

public class WokCookPhase : MonoBehaviour
{
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

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 startPos;

    [Header("Arrow System")]
    public GameObject arrowsCanvas;
    public GameObject movingArrowPrefab;
    public Transform[] fixedArrowPoints;
    public float spawnInterval = 1.5f;
    public int maxArrowsOnScreen = 3;

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

        // APAGAMOS LOS TEXTOS AL ARRANCAR
        if (hitsText != null) hitsText.gameObject.SetActive(false);
        if (errorsText != null) errorsText.gameObject.SetActive(false);
    }

    public void StartCooking()
    {
        if (arrowsCanvas != null)
        {
            arrowsCanvas.SetActive(true);
        }

        // PRENDEMOS LOS TEXTOS
        if (hitsText != null) hitsText.gameObject.SetActive(true);
        if (errorsText != null) errorsText.gameObject.SetActive(true);

        currentErrors = 0;
        currentHits = 0;
        activeArrowsCount = 0;
        spawnTimer = spawnInterval;
        startPos = wokTransform.position;
        dragPlane = new Plane(Vector3.up, dragPlaneCollider.transform.position);
        isCookingActive = true;

        UpdateUI();

        Debug.Log("Cooking started");
    }

    private void Update()
    {
        if (!isCookingActive) return;

        HandleWokDragging();
        HandleArrowSpawning();
    }

    private void HandleWokDragging()
    {
        if (Input.GetMouseButtonDown(0))
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

        currentHits++;
        UpdateUI();
        Debug.Log(currentHits + "/" + requiredHits);

        if (currentHits >= requiredHits)
        {
            WinMinigame();
        }
    }

    private void OnArrowMiss()
    {
        activeArrowsCount--;

        if (!isCookingActive) return;

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

    private void WinMinigame()
    {
        if (!isCookingActive) return;

        isCookingActive = false;

        if (arrowsCanvas != null)
        {
            arrowsCanvas.SetActive(false);
        }

        if (hitsText != null) hitsText.gameObject.SetActive(false);
        if (errorsText != null) errorsText.gameObject.SetActive(false);

        Debug.Log("Wok won");

        if (wokController != null)
        {
            wokController.FinishCooking(true);
        }
    }

    private void LoseMinigame()
    {
        if (!isCookingActive) return;

        isCookingActive = false;

        if (arrowsCanvas != null)
        {
            arrowsCanvas.SetActive(false);
        }

        if (hitsText != null) hitsText.gameObject.SetActive(false);
        if (errorsText != null) errorsText.gameObject.SetActive(false);

        Debug.Log("Wok lost");

        if (wokController != null)
        {
            wokController.FinishCooking(false);
        }
    }
}