using System.Collections;
using RestaurantChaos.Tutorial;
using TMPro;
using UnityEngine;

public class Fridge : MonoBehaviour, IInteractable, IMinigame
{
    [Header("Door")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 100f;
    [SerializeField] private float doorSpeed = 120f;

    [Header("Minigame")]
    [SerializeField] private GameObject transitionCam;
    [SerializeField] private GameObject minigameContainer;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private MinigameGuide guide;

    [Header("Lights")]
    [SerializeField] private Light[] lights;
    [SerializeField] private float lightFadeDuration = 0.6f;

    [Header("Grab")]
    [SerializeField] private float grabDistance = 0.6f;
    [SerializeField] private float grabScale = 3f;
    [SerializeField] private float grabDuration = 0.45f;

    [Header("Ingredient Label")]
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private GameObject lockLabel;
    [SerializeField] private Vector2 labelOffset = new Vector2(0f, 60f);

    [Header("References")]
    [SerializeField] private PlayerHoldSystem playerHoldSystem;
    [SerializeField] private PlayerController player;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private float[] lightTargets;
    private FridgeIngredient hovered;    private bool isDoorMoving;
    private bool isPicking;

    private void Start()
    {
        closedRotation = doorPivot.rotation;
        openRotation = Quaternion.AngleAxis(openAngle, Vector3.up) * closedRotation;

        lightTargets = new float[lights.Length];

        for (int i = 0; i < lights.Length; i++)
        {
            lightTargets[i] = lights[i].intensity;
        }

        minigameContainer.SetActive(false);
        nameLabel.gameObject.SetActive(false);

        if (lockLabel != null)
        {
            lockLabel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPicking)
        {
            return;
        }

        SetHovered(RaycastIngredient());

        if (hovered == null)
        {
            return;
        }

        Vector2 labelPosition = (Vector2)Input.mousePosition + labelOffset;

        nameLabel.rectTransform.position = labelPosition;

        if (lockLabel != null)
        {
            lockLabel.transform.position = labelPosition;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Pick(hovered);
        }
    }

    public bool CanInteract()
    {
        return !isDoorMoving && !MinigameManager.Instance.IsBusy && !playerHoldSystem.IsHoldingItem;
    }

    public void Interact()
    {
        if (CanInteract())
        {
            StartCoroutine(OpenAndEnter());
        }
    }

    public void SetupMinigame()
    {
        StartCoroutine(RunMinigame());
    }

    public void EndMinigame()
    {
        isPicking = false;

        SetHovered(null);
        minigameContainer.SetActive(false);
        SoundManager.Instance?.PlaySound(SoundType.FridgeClose);
        StartCoroutine(MoveDoor(closedRotation));
    }

    private IEnumerator OpenAndEnter()
    {
        player.SetMovement(false);

        SoundManager.Instance?.PlaySound(SoundType.FridgeOpen);

        yield return MoveDoor(openRotation);

        MinigameManager.Instance.EnterMinigame(transitionCam, this);
    }

    private IEnumerator MoveDoor(Quaternion target)
    {
        isDoorMoving = true;

        while (Quaternion.Angle(doorPivot.rotation, target) > 0.1f)
        {
            doorPivot.rotation = Quaternion.RotateTowards(doorPivot.rotation, target, doorSpeed * Time.deltaTime);

            yield return null;
        }

        doorPivot.rotation = target;
        isDoorMoving = false;
    }

    private IEnumerator RunMinigame()
    {
        Collider guidedClick = null;

        foreach (Light fridgeLight in lights)
        {
            fridgeLight.intensity = 0f;
        }

        minigameContainer.SetActive(true);
        StartCoroutine(FadeInLights());

        if (guide != null)
        {
            yield return guide.PlayIfNeeded();

            guidedClick = guide.ConsumeClickedCollider();
        }

        isPicking = true;

        if (guidedClick != null)
        {
            Pick(guidedClick.GetComponentInParent<FridgeIngredient>());
        }
    }

    private IEnumerator FadeInLights()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime / lightFadeDuration;

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = Mathf.Lerp(0f, lightTargets[i], progress);
            }

            yield return null;
        }
    }

    private FridgeIngredient RaycastIngredient()
    {
        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponentInParent<FridgeIngredient>();
        }

        return null;
    }

    private void SetHovered(FridgeIngredient target)
    {
        if (target == hovered)
        {
            return;
        }

        if (hovered != null)
        {
            hovered.SetHighlight(false);
        }

        hovered = target;

        if (hovered != null)
        {
            hovered.SetHighlight(true);
            nameLabel.text = hovered.DisplayName;
        }

        bool isLocked = hovered != null && hovered.IsLocked;

        nameLabel.gameObject.SetActive(hovered != null && !isLocked);

        if (lockLabel != null)
        {
            lockLabel.SetActive(isLocked);
        }
    }

    private void Pick(FridgeIngredient ingredient)
    {
        if (ingredient == null || ingredient.IsLocked || !playerHoldSystem.HoldItem(ingredient.HeldPrefab))
        {
            return;
        }

        isPicking = false;

        SetHovered(null);
        SoundManager.Instance?.PlaySound(SoundType.IngredientGrab);
        StartCoroutine(FlyToCamera(ingredient));
    }

    private IEnumerator FlyToCamera(FridgeIngredient ingredient)
    {
        Transform cam = worldCamera.transform;
        GameObject visual = Instantiate(ingredient.GrabVisual, ingredient.transform.position, ingredient.GrabVisual.transform.rotation);
        Vector3 startPosition = visual.transform.position;
        Vector3 startScale = visual.transform.localScale;
        Vector3 endPosition = cam.position + cam.forward * grabDistance;
        float progress = 0f;

        MinigameManager.Instance.isTransitioning = true;

        while (progress < 1f)
        {
            progress += Time.deltaTime / grabDuration;

            float eased = Mathf.SmoothStep(0f, 1f, progress);

            visual.transform.position = Vector3.Lerp(startPosition, endPosition, eased);
            visual.transform.localScale = Vector3.Lerp(startScale, startScale * grabScale, eased);

            yield return null;
        }

        Destroy(visual);

        MinigameManager.Instance.isTransitioning = false;
        MinigameManager.Instance.ExitMinigame();
    }
}
