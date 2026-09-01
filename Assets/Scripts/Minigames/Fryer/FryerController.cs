using System.Collections;
using UnityEngine;

public class FryerController : MonoBehaviour
{
    [Header("References")]
    public FryerVisuals visuals;
    public GameObject minigameContainer;
    public PlayerHoldSystem holdSystem;

    [Header("Rules")]
    public int successes = 5;
    public int fails = 3;
    public float timeBeforeSkillcheck = 3f;

    [Header("Skillcheck Config")]
    public float needleSpeed = 180f;
    [Range(0.05f, 0.5f)]
    public float zoneSizeRatio = 0.15f;
    public float minDistanceBetweenZones = 100f;

    [Header("Finished Dish Reward")]
    [SerializeField] private GameObject friedTempuraPrefab;

    private int currentSuccesses = 0;
    private int currentFails = 0;
    private bool isMinigameActive = false;
    private bool isSkillcheckActive = false;

    private float needleAngle = 0f;
    private int needleDirection = 1;
    private float currentZoneStartAngle = 0f;
    private float zoneSizeDegrees;

    private void Start()
    {
        if (minigameContainer != null)
        {
            minigameContainer.SetActive(false);
        }
    }

    public void StartMinigame()
    {
        currentSuccesses = 0;
        currentFails = 0;
        needleAngle = 0f;
        needleDirection = 1;
        isMinigameActive = true;
        isSkillcheckActive = false;

        if (minigameContainer != null)
        {
            minigameContainer.SetActive(true);
        }

        if (visuals != null)
        {
            visuals.ShowSkillcheck(false);
            visuals.ShowCountdown(false);
            visuals.UpdateHits(0, successes);
            visuals.UpdateMisses(0, fails);
        }

        StartCoroutine(FryingSequence());
    }

    public void EndMinigame()
    {
        EndMinigame(false);
    }

    public void EndMinigame(bool won)
    {
        isMinigameActive = false;
        isSkillcheckActive = false;

        if (visuals != null)
        {
            visuals.ShowSkillcheck(false);
            visuals.ShowCountdown(false);
        }

        StartCoroutine(EndSequence(won));
    }

    private IEnumerator EndSequence(bool won)
    {
        if (visuals != null) visuals.MoveBasketUp();

        yield return new WaitForSeconds(1.5f);

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ExitMinigame();
        }

        if (minigameContainer != null)
        {
            minigameContainer.SetActive(false);
        }

        if (won)
        {
            Debug.Log("Won");
            if (holdSystem != null && friedTempuraPrefab != null)
            {
                holdSystem.HoldItem(friedTempuraPrefab);
            }
        }
        else
        {
            Debug.Log("Lost");
        }
    }

    private IEnumerator FryingSequence()
    {
        yield return new WaitForSeconds(1f);
        if (visuals != null) visuals.SpawnTempuras();
        yield return new WaitForSeconds(2f);

        if (visuals != null) visuals.MoveBasketDown();

        float remainingTime = Mathf.Max(0f, timeBeforeSkillcheck - 1f);
        yield return new WaitForSeconds(remainingTime);

        zoneSizeDegrees = zoneSizeRatio * 360f;
        GenerateNewZone();

        if (visuals != null)
        {
            visuals.UpdateSkillcheckUI(needleAngle, currentZoneStartAngle, zoneSizeRatio);
            visuals.ShowSkillcheck(true);
            visuals.ShowCountdown(true);
        }

        for (int i = 3; i > 0; i--)
        {
            if (visuals != null) visuals.UpdateCountdownText(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        if (visuals != null) visuals.ShowCountdown(false);

        isSkillcheckActive = true;
    }

    private void Update()
    {
        if (!isMinigameActive || !isSkillcheckActive) return;

        needleAngle += needleSpeed * needleDirection * Time.deltaTime;

        needleAngle %= 360f;
        if (needleAngle < 0) needleAngle += 360f;

        if (visuals != null) visuals.UpdateSkillcheckUI(needleAngle, currentZoneStartAngle, zoneSizeRatio);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            CheckSkillcheck();
        }
    }

    private void CheckSkillcheck()
    {
        float endAngle = currentZoneStartAngle + zoneSizeDegrees;
        bool hit = false;

        if (endAngle > 360f)
        {
            if (needleAngle >= currentZoneStartAngle || needleAngle <= (endAngle - 360f)) hit = true;
        }
        else
        {
            if (needleAngle >= currentZoneStartAngle && needleAngle <= endAngle) hit = true;
        }

        if (hit) Success();
        else Fail();
    }

    private void Success()
    {
        currentSuccesses++;

        if (visuals != null)
        {
            visuals.UpdateHits(currentSuccesses, successes);
            visuals.BumpTempuras();
        }

        needleDirection *= -1;
        needleSpeed += 20f;

        if (currentSuccesses >= successes) EndMinigame(true);
        else GenerateNewZone();
    }

    private void Fail()
    {
        currentFails++;

        if (visuals != null)
        {
            visuals.UpdateMisses(currentFails, fails);
        }

        if (currentFails >= fails) EndMinigame(false);
    }

    private void GenerateNewZone()
    {
        float lastAngle = currentZoneStartAngle;
        float newAngle = lastAngle;
        int attempts = 0;

        while (Mathf.Abs(Mathf.DeltaAngle(lastAngle, newAngle)) < minDistanceBetweenZones && attempts < 10)
        {
            newAngle = Random.Range(0f, 360f);
            attempts++;
        }

        currentZoneStartAngle = newAngle;
    }
}