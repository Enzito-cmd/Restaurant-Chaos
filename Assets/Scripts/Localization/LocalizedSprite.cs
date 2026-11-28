using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LocalizedSprite : MonoBehaviour
{
    [SerializeField] private Sprite english;
    [SerializeField] private Sprite spanish;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        Apply();

        Localization.Changed += Apply;
    }

    private void OnDisable()
    {
        Localization.Changed -= Apply;
    }

    private void Apply()
    {
        if (Localization.Current == Language.Spanish && spanish != null)
        {
            image.sprite = spanish;
        }
        else if (english != null)
        {
            image.sprite = english;
        }
    }
}
