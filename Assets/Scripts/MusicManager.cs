using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip music;

    private const string MusicVolumeKey = "MusicVolume";
    private const string MasterVolumeKey = "MasterVolume";

    private float musicVolume = 1f;

    public float MusicVolume => musicVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadMusicVolume();
    }

    private void Start()
    {
        if (musicSource == null || music == null)
            return;

        if (musicSource.isPlaying)
            return;

        musicSource.clip = music;
        musicSource.loop = true;

        ApplyVolume();

        musicSource.Play();
    }

    // =========================================================
    // SLIDER MÚSICA
    // =========================================================

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        ApplyVolume();

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        PlayerPrefs.Save();
    }

    // =========================================================
    // MASTER
    // =========================================================

    public void ApplyMasterVolume(float masterVolume)
    {
        masterVolume = Mathf.Clamp01(masterVolume);

        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume * masterVolume;
        }
    }

    // =========================================================
    // APLICAR VOLUMEN
    // =========================================================

    private void ApplyVolume()
    {
        float masterVolume =
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                1f
            );

        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume * masterVolume;
        }
    }

    // =========================================================
    // CARGAR
    // =========================================================

    private void LoadMusicVolume()
    {
        musicVolume = PlayerPrefs.GetFloat(
            MusicVolumeKey,
            1f
        );

        ApplyVolume();
    }
}