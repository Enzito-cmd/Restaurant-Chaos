using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    public void GoToLevelSelector()
    {
        StartCoroutine(LoadSceneWithSound("LevelSelector"));
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