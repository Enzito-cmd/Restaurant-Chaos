using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineCamera diningRoomCamera;
    [SerializeField] private CinemachineCamera kitchenCamera;
    [SerializeField] private CinemachineCamera minigameCamera;

    [Header("Priority Settings")]
    [SerializeField] private int activePriority = 10;
    [SerializeField] private int inactivePriority = 0;

    private List<CinemachineCamera> allCameras;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        allCameras = new List<CinemachineCamera>
        {
            diningRoomCamera,
            kitchenCamera,
            minigameCamera
        };
    }

    private void Start()
    {
        ActivateDiningRoomCamera();
    }

    public void SwitchCamera(CinemachineCamera targetCamera)
    {
        if (targetCamera == null) return;

        foreach (var cam in allCameras)
        {
            if (cam != null)
            {
                cam.Priority.Value = inactivePriority;
            }
        }

        targetCamera.Priority.Value = activePriority;
    }

    public void ActivateDiningRoomCamera() => SwitchCamera(diningRoomCamera);
    public void ActivateKitchenCamera() => SwitchCamera(kitchenCamera);
    public void ActivateMinigameCamera() => SwitchCamera(minigameCamera);
}