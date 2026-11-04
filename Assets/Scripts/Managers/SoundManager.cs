using System;
using System.Collections.Generic;
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
    OrderPlaced,
    Stars,
    WokHit,
    ClientSpawn,
    ClientDeath,
    CookingMiss,
    DoorOpen,
    FridgeOpen,
    FridgeClose,
    IngredientGrab,
    TempuraStep,
    TempuraFail,
    TempuraComplete,
    TempuraLose,
    FryerHit,
    FryerMiss,
    FryerComplete,
    FryerLose,
    TutorialPop,
    TutorialType,
    WokFinalSkillCheck,
    ClientEat,
    PatienceLow,
    DayEnd,
    PauseOpen,
    PauseClose,
    StoveLoop,
    FryingLoop,
    EggBroken,
    FoodToss,
    PlateSlide,
    PlayerWalk,
    ExtinguisherEquip,
    ExtinguisherSpray,
    PlatePlace,
    PlateTake,
    SceneBurn,
    LockedStation,
    BookOpen,
    TrashThrow
}

[Serializable]
public class SoundEntry
{
    public SoundType type;
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitchMin = 1f;
    [Range(0.5f, 2f)] public float pitchMax = 1f;
    public float minInterval;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource soundSource;

    [Header("Sounds")]
    [SerializeField] private SoundEntry[] sounds;

    private const string SoundVolumeKey = "SoundVolume";
    private const string MasterVolumeKey = "MasterVolume";

    private readonly Dictionary<SoundType, SoundEntry> entries = new Dictionary<SoundType, SoundEntry>();
    private readonly Dictionary<SoundType, float> lastPlayTimes = new Dictionary<SoundType, float>();
    private readonly Dictionary<SoundType, AudioSource> activeLoops = new Dictionary<SoundType, AudioSource>();
    private readonly List<AudioSource> sourcePool = new List<AudioSource>();

    private float soundVolume = 1f;
    private float masterVolume = 1f;

    public float SoundVolume => soundVolume;
    public float MasterVolume => masterVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        foreach (SoundEntry entry in sounds)
        {
            entries[entry.type] = entry;
        }

        if (soundSource == null)
        {
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.playOnAwake = false;
        }

        sourcePool.Add(soundSource);

        LoadSettings();
    }

    public void PlaySound(SoundType sound)
    {
        PlaySoundAndGetLength(sound);
    }

    public float PlaySoundAndGetLength(SoundType sound)
    {
        if (!entries.TryGetValue(sound, out SoundEntry entry))
            return 0f;

        AudioClip clip = PickClip(entry);

        if (clip == null)
            return 0f;

        if (lastPlayTimes.TryGetValue(sound, out float lastTime) && Time.unscaledTime - lastTime < entry.minInterval)
            return 0f;

        lastPlayTimes[sound] = Time.unscaledTime;

        AudioSource source = GetFreeSource();

        source.loop = false;
        source.clip = clip;
        source.pitch = UnityEngine.Random.Range(entry.pitchMin, entry.pitchMax);
        source.volume = entry.volume * soundVolume * masterVolume;
        source.Play();

        return clip.length / source.pitch;
    }

    public void StartLoopingSound(SoundType sound)
    {
        if (activeLoops.ContainsKey(sound))
            return;

        if (!entries.TryGetValue(sound, out SoundEntry entry))
            return;

        AudioClip clip = PickClip(entry);

        if (clip == null)
            return;

        AudioSource source = GetFreeSource();

        source.loop = true;
        source.clip = clip;
        source.pitch = 1f;
        source.volume = entry.volume * soundVolume * masterVolume;
        source.Play();

        activeLoops[sound] = source;
    }

    public void StopLoopingSound(SoundType sound)
    {
        if (!activeLoops.TryGetValue(sound, out AudioSource source))
            return;

        activeLoops.Remove(sound);

        if (source == null)
            return;

        source.Stop();
        source.loop = false;
        source.clip = null;
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);

        UpdateLoopVolumes();

        PlayerPrefs.SetFloat(SoundVolumeKey, soundVolume);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        UpdateLoopVolumes();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ApplyMasterVolume(masterVolume);
        }

        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.Save();
    }

    private AudioClip PickClip(SoundEntry entry)
    {
        if (entry.clips == null || entry.clips.Length == 0)
            return null;

        return entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];
    }

    private AudioSource GetFreeSource()
    {
        foreach (AudioSource source in sourcePool)
        {
            if (!source.isPlaying)
                return source;
        }

        AudioSource created = gameObject.AddComponent<AudioSource>();

        created.playOnAwake = false;
        created.spatialBlend = 0f;
        created.outputAudioMixerGroup = soundSource.outputAudioMixerGroup;

        sourcePool.Add(created);

        return created;
    }

    private void UpdateLoopVolumes()
    {
        foreach (KeyValuePair<SoundType, AudioSource> loop in activeLoops)
        {
            loop.Value.volume = entries[loop.Key].volume * soundVolume * masterVolume;
        }
    }

    private void LoadSettings()
    {
        soundVolume = PlayerPrefs.GetFloat(SoundVolumeKey, 1f);
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
    }

#if UNITY_EDITOR
    [ContextMenu("Add Missing Sound Rows")]
    private void AddMissingRows()
    {
        List<SoundEntry> all = new List<SoundEntry>(sounds);

        foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
        {
            if (all.Exists(entry => entry.type == type))
                continue;

            SoundEntry added = new SoundEntry();
            added.type = type;
            all.Add(added);
        }

        sounds = all.ToArray();

        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Log Sounds Without Clips")]
    private void LogSoundsWithoutClips()
    {
        foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
        {
            SoundEntry found = Array.Find(sounds, entry => entry.type == type);

            if (found == null || found.clips == null || found.clips.Length == 0 || Array.TrueForAll(found.clips, clip => clip == null))
            {
                Debug.Log($"No clip for {type}", this);
            }
        }
    }

    private void OnValidate()
    {
        if (sounds == null)
            return;

        HashSet<SoundType> seen = new HashSet<SoundType>();

        foreach (SoundEntry entry in sounds)
        {
            if (!seen.Add(entry.type))
            {
                Debug.LogWarning($"Duplicate sound row: {entry.type}", this);
            }

            if (entry.pitchMax < entry.pitchMin)
            {
                entry.pitchMax = entry.pitchMin;
            }
        }
    }
#endif
}
