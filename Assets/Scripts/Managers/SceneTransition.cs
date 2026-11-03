using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    public static bool IsRunning => Instance != null && Instance.busy;

    [SerializeField] private RawImage screen;
    [SerializeField] private float duration = 1.2f;

    private Material burn;
    private bool busy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        burn = new Material(screen.material);
        screen.material = burn;
    }

    public static void Load(string sceneName)
    {
        if (Instance == null || Instance.screen == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        Instance.StartCoroutine(Instance.Run(sceneName));
    }

    private IEnumerator Run(string sceneName)
    {
        if (busy) yield break;

        busy = true;

        yield return new WaitForEndOfFrame();

        Texture2D snapshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        snapshot.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
        snapshot.Apply();
        screen.texture = snapshot;
        burn.SetFloat("_Progress", 0f);
        screen.gameObject.SetActive(true);

        yield return SceneManager.LoadSceneAsync(sceneName);

        for (float t = 0f; t < 1f; t += Mathf.Min(Time.unscaledDeltaTime, 0.05f) / duration)
        {
            burn.SetFloat("_Progress", t);
            yield return null;
        }

        screen.gameObject.SetActive(false);
        Destroy(snapshot);
        busy = false;
    }
}
