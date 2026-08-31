using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider masterSlider;

    private void Start()
    {
        // Valores iniciales

        if (SoundManager.Instance != null)
        {
            soundSlider.value =
                SoundManager.Instance.SoundVolume;

            masterSlider.value =
                SoundManager.Instance.MasterVolume;
        }

        if (MusicManager.Instance != null)
        {
            musicSlider.value =
                MusicManager.Instance.MusicVolume;
        }

        // Conectar sliders

        soundSlider.onValueChanged.AddListener(
            SetSoundVolume
        );

        musicSlider.onValueChanged.AddListener(
            SetMusicVolume
        );

        masterSlider.onValueChanged.AddListener(
            SetMasterVolume
        );
    }

    public void SetSoundVolume(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSoundVolume(value);
        }
    }

    public void SetMusicVolume(float value)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetMusicVolume(value);
        }
    }

    public void SetMasterVolume(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMasterVolume(value);
        }
    }
}