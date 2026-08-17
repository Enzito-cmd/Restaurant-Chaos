using UnityEngine;
using System.Collections;

public class WokPrepPhase : MonoBehaviour
{
    public enum PrepItem { None, Egg, Flour }

    [Header("References")]
    public WokController wokController;     
    public Camera wokCamera;               

    [Header("Clickable Areas (Colliders)")]
    public Collider eggBowlCollider;
    public Collider flourBowlCollider;
    public Collider wokCollider;

    [Header("Drag Visuals (Prefabs)")]
    public GameObject dragEggPrefab;        
    public GameObject dragFlourBowlPrefab;  
    public ParticleSystem flourParticles;

    public float dragHeightOffset = 1.5f;

    [Header("Progression")]
    public int requiredEggs = 2;
    public float requiredFlour = 2f;

    private int currentEggs = 0;
    private float currentFlour = 0f;

    [Header("Feedback Settings")]
    public float pourSpeed;            
    public float tiltAngle = 45f;         
    

    private PrepItem currentItem = PrepItem.None;
    private GameObject heldObjInstance;
    private ParticleSystem heldParticles;
    private Plane dragPlane;
    private bool isPrepActive = false;

    public void StartPrepPhase()
    {
        currentEggs = 0;
        currentFlour = 0f;
        currentItem = PrepItem.None;
        isPrepActive = true;
        dragPlane = new Plane(Vector3.up, wokCollider.transform.position);

        Debug.Log("Prep phase");
    }

    private void Update()
    {
        if (!isPrepActive) return;

        HandleMouseInput();
        UpdateHeldObjectPosition();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (currentItem == PrepItem.None)
                {
                    if (hit.collider == eggBowlCollider && currentEggs < requiredEggs)
                    {
                        GrabItem(PrepItem.Egg, dragEggPrefab);
                    }
                    else if (hit.collider == flourBowlCollider && currentFlour < requiredFlour)
                    {
                        GrabItem(PrepItem.Flour, dragFlourBowlPrefab);
                    }
                }
                else if (currentItem == PrepItem.Egg)
                {
                    if (hit.collider == wokCollider)
                    {
                        DropEgg();
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && currentItem == PrepItem.Flour)
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == wokCollider)
            {
                PourFlour();
            }
            else
            {
                StopPouring();
            }
        }

        if (Input.GetMouseButtonUp(0) && currentItem == PrepItem.Flour)
        {
            StopPouring();
        }

        if (Input.GetMouseButtonDown(1) && currentItem != PrepItem.None)
        {
            ClearHeldItem();
        }
    }

    private void UpdateHeldObjectPosition()
    {
        if (heldObjInstance != null)
        {
            Ray ray = wokCamera.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                // Ahora multiplicamos hacia arriba usando tu variable personalizable
                heldObjInstance.transform.position = hitPoint + Vector3.up * dragHeightOffset;
            }
        }
    }

    private void GrabItem(PrepItem item, GameObject prefab)
    {
        currentItem = item;
        heldObjInstance = Instantiate(prefab);

        if (item == PrepItem.Flour)
        {
            heldParticles = heldObjInstance.GetComponentInChildren<ParticleSystem>();
            if (heldParticles != null) heldParticles.Stop();
        }
    }

    private void ClearHeldItem()
    {
        if (heldObjInstance != null) Destroy(heldObjInstance);
        currentItem = PrepItem.None;
        heldParticles = null;
    }

    private void DropEgg()
    {
        currentEggs++;
        Debug.Log($"Eggs: {currentEggs}/{requiredEggs}");


        ClearHeldItem();
        CheckCompletion();
    }

    private void PourFlour()
    {
        if (currentFlour >= requiredFlour) return;

        heldObjInstance.transform.localRotation = Quaternion.Lerp(heldObjInstance.transform.localRotation, Quaternion.Euler(0, 0, tiltAngle), Time.deltaTime * 10f);

        if (heldParticles != null && !heldParticles.isPlaying) heldParticles.Play();

        currentFlour += Time.deltaTime * pourSpeed;
        Debug.Log($"{currentFlour.ToString("F2")} / {requiredFlour}");

        if (currentFlour >= requiredFlour)
        {
            currentFlour = requiredFlour;
            Debug.Log("Flour full");
            StopPouring();
            ClearHeldItem();
            CheckCompletion();
        }
    }

    private void StopPouring()
    {
        if (heldObjInstance != null)
        {
            heldObjInstance.transform.localRotation = Quaternion.Lerp(heldObjInstance.transform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
        if (heldParticles != null && heldParticles.isPlaying) heldParticles.Stop();
    }

    private void CheckCompletion()
    {
        if (currentEggs >= requiredEggs && currentFlour >= requiredFlour)
        {
            isPrepActive = false;
            Debug.Log("Completed");
            // wokController.StartCookingPhase(); 
        }
    }
}