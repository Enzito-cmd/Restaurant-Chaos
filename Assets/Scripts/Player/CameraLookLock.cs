using RestaurantChaos.Tutorial;
using UnityEngine;
using Unity.Cinemachine;

public class CameraLookLock : MonoBehaviour
{
    public static CameraLookLock Instance { get; private set; }

    private CinemachineInputAxisController inputAxisController;
    private CinemachineCamera virtualCamera;
    private CinemachineOrbitalFollow orbitalFollow;
    private float horizontalDefault;
    private float verticalDefault;
    private float radialDefault;

    private void Awake()
    {
        Instance = this;

        inputAxisController = GetComponent<CinemachineInputAxisController>();
        virtualCamera = GetComponent<CinemachineCamera>();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        if (orbitalFollow != null)
        {
            horizontalDefault = orbitalFollow.HorizontalAxis.Value;
            verticalDefault = orbitalFollow.VerticalAxis.Value;
            radialDefault = orbitalFollow.RadialAxis.Value;
        }
    }

    public void ResetCamera()
    {
        if (orbitalFollow == null) return;

        orbitalFollow.HorizontalAxis.Value = horizontalDefault;
        orbitalFollow.VerticalAxis.Value = verticalDefault;
        orbitalFollow.RadialAxis.Value = radialDefault;

        if (virtualCamera != null)
        {
            virtualCamera.PreviousStateIsValid = false;
        }
    }

    private void Update()
    {
        if (inputAxisController == null) return;

        bool isBlockedByUi = Cursor.visible || TutorialSignUI.IsAnySignShowing;

        inputAxisController.enabled = !isBlockedByUi && Input.GetMouseButton(1);
    }
}
