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
    Stars
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

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusic;
    private void Start()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    private void Awake()
    {
        Instance = this;
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
        }

        return null;
    }
}