using System.Collections;
using RestaurantChaos.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TempuraController : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private MinigameGuide guide;

    [Header("References")]
    [SerializeField] private TempuraGridLogic logic;
    [SerializeField] private TempuraGridVisuals visuals;
    [SerializeField] private PlayerHoldSystem playerHoldSystem;
    [SerializeField] private GameObject tempuraTray;
    [SerializeField] private GameObject breadedTempuraPrefab;

    [Header("Rules")]
    public int roundsToWin = 3;
    [SerializeField] private int maxMistakes = 3;

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [FormerlySerializedAs("remainingText")]
    [SerializeField] private TMP_Text completedText;
    [SerializeField] private TMP_Text mistakesText;

    [Header("Input Tolerance")]
    [SerializeField] private float diagonalWindow = 0.15f;
    [SerializeField] private float failDelay = 0.8f;

    private int currentRound = 0;
    private int currentMistakes = 0;
    private int currentStepIndex = 0;
    private Vector2Int playerPos;
    private bool isPlaying = false;
    private Coroutine startRoutine;
    private Coroutine failRoutine;

    private bool isWaitingForInputBuffer = false;
    private float bufferDeadline;
    private bool bufferW, bufferS, bufferA, bufferD;

    public void StartMinigame()
    {
        currentRound = 0;
        currentMistakes = 0;
        StartRound();
        tempuraTray.SetActive(true);
        panel.SetActive(true);
        UpdatePanel();

        startRoutine = StartCoroutine(BeginPlaying());
    }

    private IEnumerator BeginPlaying()
    {
        if (guide != null)
        {
            yield return guide.PlayIfNeeded();
        }

        startRoutine = null;
        isPlaying = true;
    }

    public void EndMinigame()
    {
        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        if (failRoutine != null)
        {
            StopCoroutine(failRoutine);
            failRoutine = null;
        }

        tempuraTray.SetActive(false);
        panel.SetActive(false);
        isPlaying = false;
        isWaitingForInputBuffer = false;
        ResetBuffers();
        visuals.ClearGrid();
    }

    private void StartRound()
    {
        logic.GenerateNewGrid();
        visuals.SpawnGrid(logic.gridLogic, logic.width, logic.height);

        currentStepIndex = 0;
        playerPos = logic.currentPath[0];
        ResetBuffers();
        isWaitingForInputBuffer = false;

        logic.gridLogic[playerPos.x, playerPos.y] = TempuraGridLogic.TileType.Passed;
        visuals.UpdateTile(playerPos.x, playerPos.y, TempuraGridLogic.TileType.Passed);

        visuals.SpawnArrow();
        UpdateArrow();

        visuals.SetBreading(0f);
        visuals.SpawnTempura(playerPos.x, playerPos.y);
    }

    private void UpdateBreading()
    {
        visuals.SetBreading((float)currentStepIndex / Mathf.Max(logic.currentPath.Count - 1, 1));
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (Input.GetKeyDown(KeyCode.W)) { bufferW = true; StartInputBuffer(); }
        if (Input.GetKeyDown(KeyCode.S)) { bufferS = true; StartInputBuffer(); }
        if (Input.GetKeyDown(KeyCode.A)) { bufferA = true; StartInputBuffer(); }
        if (Input.GetKeyDown(KeyCode.D)) { bufferD = true; StartInputBuffer(); }

        if (!isWaitingForInputBuffer) return;

        Vector2Int moveInput = ReadBufferedMove();

        if (IsNextStep(playerPos + moveInput) || Time.time >= bufferDeadline)
        {
            EvaluateBufferedInput(moveInput);
        }
    }

    private void StartInputBuffer()
    {
        if (isWaitingForInputBuffer) return;

        isWaitingForInputBuffer = true;
        bufferDeadline = Time.time + diagonalWindow;
    }

    private Vector2Int ReadBufferedMove()
    {
        Vector2Int moveInput = Vector2Int.zero;

        if (bufferW) moveInput.y += 1;
        if (bufferS) moveInput.y -= 1;
        if (bufferD) moveInput.x += 1;
        if (bufferA) moveInput.x -= 1;

        return moveInput;
    }

    private bool IsNextStep(Vector2Int position)
    {
        return currentStepIndex + 1 < logic.currentPath.Count && position == logic.currentPath[currentStepIndex + 1];
    }

    private void EvaluateBufferedInput(Vector2Int moveInput)
    {
        isWaitingForInputBuffer = false;

        ResetBuffers();

        if (moveInput != Vector2Int.zero)
        {
            TryMove(playerPos + moveInput);
        }
    }

    private void ResetBuffers()
    {
        bufferW = false;
        bufferS = false;
        bufferA = false;
        bufferD = false;
    }

    private void TryMove(Vector2Int newPos)
    {
        if (newPos.x < 0 || newPos.x >= logic.width || newPos.y < 0 || newPos.y >= logic.height)
        {
            FailRound();
            return;
        }

        if (IsNextStep(newPos))
        {
            currentStepIndex++;
            playerPos = newPos;

            bool reachedEnd = (currentStepIndex == logic.currentPath.Count - 1);

            if (!reachedEnd)
            {
                logic.gridLogic[playerPos.x, playerPos.y] = TempuraGridLogic.TileType.Passed;
            }

            visuals.UpdateTile(playerPos.x, playerPos.y, logic.gridLogic[playerPos.x, playerPos.y]);
            SoundManager.Instance?.PlaySound(SoundType.TempuraStep);
            UpdateBreading();
            UpdateArrow();
            visuals.UpdateTempuraTarget(playerPos.x, playerPos.y);
            if (reachedEnd)
            {
                WinRound();
            }
        }
        else
        {
            FailRound();
        }
    }
    private void UpdateArrow()
    {
        bool isLastStep = (currentStepIndex >= logic.currentPath.Count - 1);

        if (isLastStep)
        {
            visuals.UpdateArrowIndicator(playerPos.x, playerPos.y, playerPos.x, playerPos.y, true);
            return;
        }

        Vector2Int nextPos = logic.currentPath[currentStepIndex + 1];
        visuals.UpdateArrowIndicator(playerPos.x, playerPos.y, nextPos.x, nextPos.y, false);
    }

    private void FailRound()
    {
        currentMistakes++;
        UpdatePanel();

        if (currentMistakes >= maxMistakes)
        {
            SoundManager.Instance?.PlaySound(SoundType.TempuraLose);
            LoseMinigame();
            return;
        }

        SoundManager.Instance?.PlaySound(SoundType.TempuraFail);

        failRoutine = StartCoroutine(RestartAfterFail());
    }

    private IEnumerator RestartAfterFail()
    {
        isPlaying = false;
        isWaitingForInputBuffer = false;
        ResetBuffers();

        yield return new WaitForSeconds(failDelay);

        StartRound();

        failRoutine = null;
        isPlaying = true;
    }

    private void LoseMinigame()
    {
        isPlaying = false;
        isWaitingForInputBuffer = false;
        ResetBuffers();

        if (MinigameManager.Instance != null)
        {
            MinigameManager.Instance.ExitMinigame();
        }
    }

    private void UpdatePanel()
    {
        completedText.text = $"{currentRound}/{roundsToWin}";
        mistakesText.text = $"{currentMistakes}/{maxMistakes}";
    }

    private void WinRound()
    {
        currentRound++;
        UpdatePanel();

        if (currentRound >= roundsToWin)
        {
            isPlaying = false;

            SoundManager.Instance?.PlaySound(SoundType.TempuraComplete);

            playerHoldSystem.ClearHeldItem();
            playerHoldSystem.HoldItem(breadedTempuraPrefab);

            if (MinigameManager.Instance != null)
            {
                MinigameManager.Instance.ExitMinigame();
            }
        }
        else
        {
            StartRound();
        }
    }
}