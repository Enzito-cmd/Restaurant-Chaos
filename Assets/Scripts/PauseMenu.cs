using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject configurationsPanel;

    private bool isPaused = false;
    private bool cursorWasVisible;

    private void Awake()
    {
        // SIEMPRE empiezan ocultos
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (configurationsPanel != null)
            configurationsPanel.SetActive(false);
    }

    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                // Si estamos en configuraciones,
                // ESC vuelve al menú de pausa
                if (configurationsPanel != null &&
                    configurationsPanel.activeSelf)
                {
                    CloseConfigurations();
                }
                else
                {
                    ResumeGame();
                }
            }
        }
    }

    // =====================================================
    // PAUSE
    // =====================================================

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        cursorWasVisible = Cursor.visible;

        if (configurationsPanel != null)
            configurationsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;

        if (CursorManager.Instance != null)
            CursorManager.Instance.ShowCursor();
    }

    // =====================================================
    // RESUME
    // =====================================================

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (configurationsPanel != null)
            configurationsPanel.SetActive(false);

        Time.timeScale = 1f;

        if (CursorManager.Instance != null)
        {
            if (cursorWasVisible)
                CursorManager.Instance.ShowCursor();
            else
                CursorManager.Instance.HideCursor();
        }
    }

    // =====================================================
    // CONFIGURATIONS
    // =====================================================

    public void OpenConfigurations()
    {
        if (!isPaused)
            return;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (configurationsPanel != null)
            configurationsPanel.SetActive(true);
    }

    // =====================================================
    // BACK
    // =====================================================

    public void CloseConfigurations()
    {
        if (!isPaused)
            return;

        if (configurationsPanel != null)
            configurationsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    // =====================================================
    // EXIT
    // =====================================================

    public void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}