using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public void PlayClick()
    {
        SoundManager.Instance?.PlaySound(SoundType.ButtonClick);
    }
}