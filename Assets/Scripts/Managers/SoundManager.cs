using UnityEngine;

public enum SoundType
{
    ButtonClick,
    ClientPicked,
    ClientSit,
    FoodDelivered,
    ClientLeave,
    ClientAngry,
    MoneySpawn,
    MoneyPickup,
    WokStart,
    EggAdded,
    RicePour,
    CookingSuccess,
    CookingFail,
    pedido,
    Stars,
    WokHit,
    ClientSpawn,
    ClientDeath,
    CookingMiss
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource soundSource;

    [Header("Sounds")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip clientPicked;
    [SerializeField] private AudioClip clientSit;
    [SerializeField] private AudioClip foodDelivered;
    [SerializeField] private AudioClip clientLeave;
    [SerializeField] private AudioClip clientAngry;
    [SerializeField] private AudioClip moneySpawn;
    [SerializeField] private AudioClip moneyPickup;
    [SerializeField] private AudioClip wokStart;
    [SerializeField] private AudioClip eggAdded;
    [SerializeField] private AudioClip ricePour;
    [SerializeField] private AudioClip cookingSuccess;
    [SerializeField] private AudioClip cookingFail;
    [SerializeField] private AudioClip pedido;
    [SerializeField] private AudioClip Stars;
    [SerializeField] private AudioClip WokHit;

    [SerializeField] private AudioClip clientSpawn;
    [SerializeField] private AudioClip clientDeath;
    [SerializeField] private AudioClip cookingMiss;

    private const string SoundVolumeKey = "SoundVolume";
    private const string MasterVolumeKey = "MasterVolume";

    private float soundVolume = 1f;
    private float masterVolume = 1f;

    public float SoundVolume => soundVolume;
    public float MasterVolume => masterVolume;
    private void Awake()
    {
        

        Instance = this;

        // Mantener entre escenas
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }
    public void PlaySound(SoundType sound)
    {
        if (soundSource == null)
            return;

        AudioClip clip = GetClip(sound);

        if (clip != null)
        {
            soundSource.PlayOneShot(clip);
        }
    }
    public void StartLoopingSound(SoundType sound)
    {
        if (soundSource == null)
            return;

        AudioClip clip = GetClip(sound);

        if (clip == null)
            return;

        if (soundSource.isPlaying && soundSource.clip == clip)
            return;

        soundSource.clip = clip;
        soundSource.loop = true;

        UpdateSoundVolume();

        soundSource.Play();
    }

    public void StopLoopingSound()
    {
        if (soundSource == null)
            return;

        soundSource.Stop();
        soundSource.clip = null;
        soundSource.loop = false;
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);

        UpdateSoundVolume();

        PlayerPrefs.SetFloat(
            SoundVolumeKey,
            soundVolume
        );

        PlayerPrefs.Save();
    }
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        // Afectar sonidos
        UpdateSoundVolume();

        // Afectar música
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ApplyMasterVolume(masterVolume);
        }

        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        PlayerPrefs.Save();
    }
    private void UpdateSoundVolume()
    {
        if (soundSource != null)
        {
            soundSource.volume =
                soundVolume * masterVolume;
        }
    }
    private void LoadSettings()
    {
        soundVolume = PlayerPrefs.GetFloat(
            SoundVolumeKey,
            1f
        );

        masterVolume = PlayerPrefs.GetFloat(
            MasterVolumeKey,
            1f
        );

        UpdateSoundVolume();
    }
    private AudioClip GetClip(SoundType sound)
    {
        switch (sound)
        {
            case SoundType.ButtonClick:
                return buttonClick;

            case SoundType.ClientPicked:
                return clientPicked;

            case SoundType.ClientSit:
                return clientSit;

            case SoundType.FoodDelivered:
                return foodDelivered;

            case SoundType.ClientLeave:
                return clientLeave;

            case SoundType.ClientAngry:
                return clientAngry;

            case SoundType.MoneySpawn:
                return moneySpawn;

            case SoundType.MoneyPickup:
                return moneyPickup;

            case SoundType.WokStart:
                return wokStart;

            case SoundType.EggAdded:
                return eggAdded;

            case SoundType.RicePour:
                return ricePour;

            case SoundType.CookingSuccess:
                return cookingSuccess;

            case SoundType.CookingFail:
                return cookingFail;

            case SoundType.pedido:
                return pedido;

            case SoundType.Stars:
                return Stars;

            case SoundType.WokHit:
                return WokHit;
            case SoundType.ClientSpawn:
                return clientSpawn;

            case SoundType.ClientDeath:
                return clientDeath;
            case SoundType.CookingMiss:
                return cookingMiss;
        }

        return null;
    }
}