using RestaurantChaos.Tutorial;
using UnityEngine;

public class HideWhileBlockingUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject visualRoot;

    private bool wasBlockingUiOpen;

    private void Update()
    {
        if (visualRoot == null) return;

        bool isBlockingUiOpen = Cursor.visible || TutorialSignUI.IsScreenCovered;

        if (isBlockingUiOpen == wasBlockingUiOpen) return;

        wasBlockingUiOpen = isBlockingUiOpen;
        visualRoot.SetActive(!isBlockingUiOpen);
    }
}
