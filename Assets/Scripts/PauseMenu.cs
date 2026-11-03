using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private UIPanelPop pausePanel;
    [SerializeField] private UIPanelPop buttonsPanel;
    [SerializeField] private UIPanelPop configurationsPanel;

    private bool isPaused = false;
    private bool cursorWasVisible;

    private void Awake()
    {
        // SIEMPRE empiezan ocultos
        if (pausePanel != null)
            pausePanel.HideInstantly();

        if (buttonsPanel != null)
            buttonsPanel.HideInstantly();

        if (configurationsPanel != null)
            configurationsPanel.HideInstantly();
    }

    private void Start()
    {
        WorldTime.SetMenuPaused(false);
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
                // ESC vuelve al men� de pausa
                if (configurationsPanel != null &&
                    configurationsPanel.IsOpen)
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

        SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

        isPaused = true;

        cursorWasVisible = Cursor.visible;

        if (configurationsPanel != null)
            configurationsPanel.HideInstantly();

        if (buttonsPanel != null)
            buttonsPanel.Show();

        if (pausePanel != null)
            pausePanel.Show();

        WorldTime.SetMenuPaused(true);

        if (CursorManager.Instance != null)
            CursorManager.Instance.ShowCursor();
    }

    // =====================================================
    // RESUME
    // =====================================================

    public void ResumeGame()
    {
        SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

        isPaused = false;

        if (pausePanel != null)
            pausePanel.Hide();

        if (buttonsPanel != null)
            buttonsPanel.Hide();

        if (configurationsPanel != null)
            configurationsPanel.Hide();

        WorldTime.SetMenuPaused(false);

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

        SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

        if (buttonsPanel != null)
            buttonsPanel.Hide();

        if (configurationsPanel != null)
            configurationsPanel.Show();
    }

    // =====================================================
    // BACK
    // =====================================================

    public void CloseConfigurations()
    {
        if (!isPaused)
            return;

        SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

        if (configurationsPanel != null)
            configurationsPanel.Hide();

        if (buttonsPanel != null)
            buttonsPanel.Show();
    }

    // =====================================================
    // EXIT
    // =====================================================

    public void GoBackToMenu()
    {
        SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

        WorldTime.Reset();
        SceneTransition.Load("Menu");
    }
}