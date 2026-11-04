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

    [SerializeField] private WokTossPhase tossPhase;

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

    [Header("Cursor")]
    [SerializeField] private CursorStyle idleCursor = CursorStyle.OpenHand;
    [SerializeField] private CursorStyle grabCursor = CursorStyle.ClosedHand;

    private PrepItem currentItem = PrepItem.None;
    private GameObject heldObjInstance;
    private ParticleSystem heldParticles;
    private Plane dragPlane;
    private bool isPrepActive = false;
    private bool isPlayingRiceSound = false;
    private bool isRiceSoundPlaying = false;
    private Coroutine transitionCoroutine;
    public void StartPrepPhase()
    {
        currentEggs = 0;
        currentRice = 0f;
        ClearHeldItem();

        if (tossPhase != null)
        {
            tossPhase.ResetState();
        }

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
        AutoDropEgg();
        UpdateHeldObjectPosition();
        UpdateCursor();
    }

    private void AutoDropEgg()
    {
        if (currentItem != PrepItem.Egg) return;

        Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == wokCollider)
        {
            DropEgg();
        }
    }

    public void GrabFromCollider(Collider clicked)
    {
        if (clicked == null || !isPrepActive || currentItem != PrepItem.None)
        {
            return;
        }

        if (clicked == eggBowlCollider && currentEggs < requiredEggs)
        {
            GrabItem(PrepItem.Egg, dragEggPrefab);
        }
        else if (clicked == riceBowlCollider && currentRice < requiredRice)
        {
            GrabItem(PrepItem.Rice, dragRiceBowlPrefab);
        }
    }

    private void UpdateCursor()
    {
        if (!isPrepActive)
        {
            return;
        }

        if (currentItem != PrepItem.None)
        {
            CursorManager.Request(grabCursor);
        }
        else
        {
            CursorManager.Request(idleCursor);
        }
    }

    private void StartRiceSound()
    {
        if (isRiceSoundPlaying)
            return;

        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.StartLoopingSound(SoundType.RicePour);

        isRiceSoundPlaying = true;
    }

    private void StopRiceSound()
    {
        if (!isRiceSoundPlaying)
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopLoopingSound(SoundType.RicePour);
        }

        isRiceSoundPlaying = false;
    }
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1) && currentItem != PrepItem.None)
        {
            DiscardHeldItem();
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
                        DiscardHeldItem();
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

    private void DiscardHeldItem()
    {
        if (currentItem == PrepItem.Egg)
        {
            SoundManager.Instance?.PlaySound(SoundType.EggBroken);
        }

        ClearHeldItem();
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
        SoundManager.Instance?.PlaySound(SoundType.EggAdded);
        UpdateUI();
        Debug.Log($"Eggs: {currentEggs}/{requiredEggs}");

        ClearHeldItem();
        CheckCompletion();
    }

    private void PourRice()
    {
        if (currentRice >= requiredRice || heldObjInstance == null)
        {
            StopRiceSound();
            return;
        }

        StartRiceSound();
        if (currentRice >= requiredRice || heldObjInstance == null) return;

        if (!isPlayingRiceSound)
        {
            isPlayingRiceSound = true;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound(SoundType.RicePour);
            }
        }

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
            StopRiceSound();
            ClearHeldItem();
            CheckCompletion();
        }
    }

    private void StopPouring()
    {
        StopRiceSound();
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

            if (tossPhase != null)
            {
                tossPhase.ShowRaw();
            }

            transitionCoroutine = StartCoroutine(TransitionToCooking());
        }
    }

    private IEnumerator TransitionToCooking()
    {
        yield return new WaitForSeconds(1f);

        transitionCoroutine = null;

        if (prepUIPanel != null) prepUIPanel.SetActive(false);
        if (wokController != null) wokController.CookPhase();
    }

    public void StopPrepPhase()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        isPrepActive = false;
        StopRiceSound();
        ClearHeldItem();

        if (tossPhase != null)
        {
            tossPhase.ResetState();
        }

        if (prepUIPanel != null) prepUIPanel.SetActive(false);
    }
}