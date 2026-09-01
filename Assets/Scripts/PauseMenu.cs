using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;
    private bool cursorWasVisible;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        // Recordamos cómo estaba el cursor ANTES de pausar
        cursorWasVisible = Cursor.visible;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.ShowCursor();
        }
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        // Restauramos el estado anterior
        if (CursorManager.Instance != null)
        {
            if (cursorWasVisible)
            {
                CursorManager.Instance.ShowCursor();
            }
            else
            {
                CursorManager.Instance.HideCursor();
            }
        }
    }
}