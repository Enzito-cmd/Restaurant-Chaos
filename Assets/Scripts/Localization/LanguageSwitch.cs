using TMPro;
using UnityEngine;

public class LanguageSwitch : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        Refresh();

        Localization.Changed += Refresh;
    }

    private void OnDisable()
    {
        Localization.Changed -= Refresh;
    }

    public void Toggle()
    {
        SoundManager.Instance?.PlaySound(SoundType.ButtonClick);

        if (Localization.Current == Language.English)
        {
            Localization.Set(Language.Spanish);
        }
        else
        {
            Localization.Set(Language.English);
        }
    }

    private void Refresh()
    {
        if (label == null) return;

        if (Localization.Current == Language.Spanish)
        {
            label.text = "Español";
        }
        else
        {
            label.text = "English";
        }
    }
}
