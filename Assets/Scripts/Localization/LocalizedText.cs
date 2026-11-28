using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField, TextArea] private string english;
    [SerializeField, TextArea] private string spanish;

    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
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
        label.text = Localization.Pick(english, spanish);
    }
}
