using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WokPrepPhase : MonoBehaviour
{
    public enum PrepItem { None, Egg, Rice }

    [Header("References")]
    public WokController wokController;
    public Camera wokCamera;

    [Header("UI References")]
    [SerializeField] private GameObject prepUIPanel;
    [SerializeField] private TMP_Text eggCounterText;
    [SerializeField] private Image riceFillImage;

    [Header("Clickable Areas")]
    public Collider eggBowlCollider;
    public Collider riceBowlCollider;
    public Collider wokCollider;

    [Header("Drag Visuals")]
    public GameObject dragEggPrefab;
    public GameObject dragRiceBowlPrefab;

    public float dragHeightOffset = 1.5f;

    [Header("Progression")]
    public int requiredEggs = 2;
    public float requiredRice = 2f;

    [SerializeField] private int currentEggs = 0;
    [SerializeField] private float currentRice = 0f;

    [Header("Feedback Settings")]
    public float pourSpeed;
    public float tiltAngle;

    private PrepItem currentItem = PrepItem.None;
    private GameObject heldObjInstance;
    private ParticleSystem heldParticles;
    private Plane dragPlane;
    private bool isPrepActive = false;

    public void StartPrepPhase()
    {
        currentEggs = 0;
        currentRice = 0f;
        ClearHeldItem();
        isPrepActive = true;
        dragPlane = new Plane(Vector3.up, wokCollider.transform.position);

        if (prepUIPanel != null) prepUIPanel.SetActive(true);
        UpdateUI();

        Debug.Log("Prep phase started");
    }

    private void Update()
    {
        if (!isPrepActive) return;

        HandleMouseInput();
        UpdateHeldObjectPosition();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1) && currentItem != PrepItem.None)
        {
            ClearHeldItem();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (currentItem == PrepItem.None)
                {
                    if (hit.collider == eggBowlCollider && currentEggs < requiredEggs)
                    {
                        GrabItem(PrepItem.Egg, dragEggPrefab);
                    }
                    else if (hit.collider == riceBowlCollider && currentRice < requiredRice)
                    {
                        GrabItem(PrepItem.Rice, dragRiceBowlPrefab);
                    }
                }
                else if (currentItem == PrepItem.Egg)
                {
                    if (hit.collider == wokCollider)
                    {
                        DropEgg();
                    }
                    else
                    {
                        ClearHeldItem();
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && currentItem == PrepItem.Rice)
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == wokCollider)
            {
                PourRice();
            }
            else
            {
                StopPouring();
            }
        }

        if (Input.GetMouseButtonUp(0) && currentItem == PrepItem.Rice)
        {
            StopPouring();
            ClearHeldItem();
        }
    }

    private void UpdateHeldObjectPosition()
    {
        if (heldObjInstance != null)
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                heldObjInstance.transform.position = hitPoint + Vector3.up * dragHeightOffset;
            }
        }
    }

    private void GrabItem(PrepItem item, GameObject prefab)
    {
        ClearHeldItem();
        currentItem = item;
        heldObjInstance = Instantiate(prefab);

        if (item == PrepItem.Rice)
        {
            heldParticles = heldObjInstance.GetComponentInChildren<ParticleSystem>();
            if (heldParticles != null)
            {
                heldParticles.Play();
                var em = heldParticles.emission;
                em.enabled = false;
            }
        }
    }

    private void ClearHeldItem()
    {
        if (heldObjInstance != null) Destroy(heldObjInstance);
        currentItem = PrepItem.None;
        heldParticles = null;
    }

    private void DropEgg()
    {
        currentEggs++;
        UpdateUI();
        Debug.Log($"Eggs: {currentEggs}/{requiredEggs}");

        ClearHeldItem();
        CheckCompletion();
    }

    private void PourRice()
    {
        if (currentRice >= requiredRice || heldObjInstance == null) return;

        Vector3 directionToCenter = (wokCollider.transform.position - heldObjInstance.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCenter) * Quaternion.Euler(tiltAngle, 0, 0);

        heldObjInstance.transform.rotation = Quaternion.Lerp(heldObjInstance.transform.rotation, targetRotation, Time.deltaTime * 10f);

        if (heldParticles != null)
        {
            var em = heldParticles.emission;
            em.enabled = true;
        }

        currentRice += Time.deltaTime * pourSpeed;
        UpdateUI();

        if (currentRice >= requiredRice)
        {
            currentRice = requiredRice;
            UpdateUI();
            StopPouring();
            ClearHeldItem();
            CheckCompletion();
        }
    }

    private void StopPouring()
    {
        if (heldObjInstance != null)
        {
            heldObjInstance.transform.rotation = Quaternion.Lerp(heldObjInstance.transform.rotation, Quaternion.identity, Time.deltaTime * 10f);
        }
        if (heldParticles != null)
        {
            var em = heldParticles.emission;
            em.enabled = false;
        }
    }

    private void UpdateUI()
    {
        if (eggCounterText != null)
        {
            eggCounterText.text = $"{currentEggs}/{requiredEggs}";
        }

        if (riceFillImage != null && requiredRice > 0)
        {
            riceFillImage.fillAmount = currentRice / requiredRice;
        }
    }

    private void CheckCompletion()
    {
        if (currentEggs >= requiredEggs && currentRice >= requiredRice)
        {
            isPrepActive = false;
            ClearHeldItem();
            StartCoroutine(TransitionToCooking());
        }
    }

    private IEnumerator TransitionToCooking()
    {
        yield return new WaitForSeconds(1f);

        if (prepUIPanel != null) prepUIPanel.SetActive(false);
        if (wokController != null) wokController.CookPhase();
    }
}