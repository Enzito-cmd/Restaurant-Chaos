using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using RestaurantChaos.Clients;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private float sceneLoadDelay = 0.2f;

    public void LoadLevel1()
    {
        StartCoroutine(LoadSceneWithSound("Level 1"));
    }

    public void LoadLevel2()
    {
        StartCoroutine(LoadSceneWithSound("Level 2"));
    }

    public void LoadGameScene()
    {
        StartCoroutine(LoadSceneWithSound("GameScene"));
    }

    public void PlayDay1()
    {
        DayManager.RequestedStartDayIndex = 0;
        StartCoroutine(LoadSceneWithSound("GameScene"));
    }

    public void PlayDay2()
    {
        DayManager.RequestedStartDayIndex = 1;
        StartCoroutine(LoadSceneWithSound("GameScene"));
    }

    public void GoToLevelSelector()
    {
        StartCoroutine(LoadSceneWithSound("LevelSelector"));
    }
    public void Configuration()
    {
        StartCoroutine(LoadSceneWithSound("Configuration"));
    }
    public void GoToMainMenu()
    {
        StartCoroutine(LoadSceneWithSound("Menu"));
    }

    public void ExitGame()
    {
        StartCoroutine(ExitGameCoroutine());
    }

    private IEnumerator ExitGameCoroutine()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.ButtonClick);
        }

        yield return new WaitForSeconds(0.3f);

        Debug.Log("Saliendo del juego...");

        Application.Quit();
    }
    private IEnumerator LoadSceneWithSound(string sceneName)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.ButtonClick);
        }

        yield return new WaitForSeconds(sceneLoadDelay);

        SceneManager.LoadScene(sceneName);
    }
}