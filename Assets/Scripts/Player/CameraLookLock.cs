using RestaurantChaos.Tutorial;
using UnityEngine;
using Unity.Cinemachine;

public class CameraLookLock : MonoBehaviour
{
    private CinemachineInputAxisController inputAxisController;

    private void Awake()
    {
        inputAxisController = GetComponent<CinemachineInputAxisController>();
    }

    private void Update()
    {
        if (inputAxisController == null) return;

        bool isBlockedByUi = Cursor.visible || TutorialSignUI.IsAnySignShowing;

        inputAxisController.enabled = !isBlockedByUi && Input.GetMouseButton(1);
    }
}
