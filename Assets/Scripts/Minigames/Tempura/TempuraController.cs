using UnityEngine;

public class TempuraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TempuraGridLogic logic;
    [SerializeField] private TempuraGridVisuals visuals;
    [SerializeField] private PlayerHoldSystem playerHoldSystem;
    [SerializeField] private GameObject tempuraTray;
    [SerializeField] private GameObject breadedTempuraPrefab;

    [Header("Rules")]
    public int roundsToWin = 3;

    [Header("Input Tolerance")]
    [SerializeField] private float diagonalBufferTime = 0.05f;

    private int currentRound = 0;
    private int currentStepIndex = 0;
    private Vector2Int playerPos;
    private bool isPlaying = false;

    private bool isWaitingForInputBuffer = false;
    private bool bufferW, bufferS, bufferA, bufferD;

    public void StartMinigame()
    {
        currentRound = 0;
        StartRound();
        tempuraTray.SetActive(true);
        isPlaying = true;
    }

    public void EndMinigame()
    {
        tempuraTray.SetActive(false);
        isPlaying = false;
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

        visuals.SpawnTempura(playerPos.x, playerPos.y);
    }

    private void Update()
    {
        if (!isPlaying) return;

        if (Input.GetKeyDown(KeyCode.W)) { bufferW = true; StartInputBuffer(); }
        if (Input.GetKeyDown(KeyCode.S)) { bufferS = true; StartInputBuffer(); }
        if (Input.GetKeyDown(KeyCode.A)) { bufferA = true; StartInputBuffer(); }
        if (Input.GetKeyDown(KeyCode.D)) { bufferD = true; StartInputBuffer(); }
    }

    private void StartInputBuffer()
    {
        if (isWaitingForInputBuffer) return;

        isWaitingForInputBuffer = true;

        Invoke(nameof(EvaluateBufferedInput), diagonalBufferTime);
    }

    private void EvaluateBufferedInput()
    {
        isWaitingForInputBuffer = false;

        if (!isPlaying)
        {
            ResetBuffers();
            return;
        }

        Vector2Int moveInput = Vector2Int.zero;

        if (bufferW) moveInput.y += 1;
        if (bufferS) moveInput.y -= 1;
        if (bufferD) moveInput.x += 1;
        if (bufferA) moveInput.x -= 1;

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

        if (currentStepIndex + 1 < logic.currentPath.Count && newPos == logic.currentPath[currentStepIndex + 1])
        {
            currentStepIndex++;
            playerPos = newPos;

            bool reachedEnd = (currentStepIndex == logic.currentPath.Count - 1);

            if (!reachedEnd)
            {
                logic.gridLogic[playerPos.x, playerPos.y] = TempuraGridLogic.TileType.Passed;
            }

            visuals.UpdateTile(playerPos.x, playerPos.y, logic.gridLogic[playerPos.x, playerPos.y]);
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
        StartRound();
    }

    private void WinRound()
    {
        currentRound++;
        Debug.Log($"{currentRound} / {roundsToWin}");

        if (currentRound >= roundsToWin)
        {
            isPlaying = false;

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