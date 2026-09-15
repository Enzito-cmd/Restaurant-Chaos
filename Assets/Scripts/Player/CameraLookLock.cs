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

        inputAxisController.enabled = !Cursor.visible;
    }
}
