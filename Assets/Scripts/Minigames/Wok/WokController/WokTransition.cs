using System.Collections;
using UnityEngine;

public class WokTransition : MonoBehaviour
{
    [Header("Bowl References (Visuals)")]
    [SerializeField] private Transform eggBowl;
    [SerializeField] private Transform riceBowl;
    [SerializeField] private Transform eggHidePoint;
    [SerializeField] private Transform riceHidePoint;
    [SerializeField] private float animationDuration = 0.6f;

    private WokCookPhase cookPhase;

    private Vector3 eggStartPosition;
    private Vector3 riceStartPosition;

    private void Awake()
    {
        cookPhase = GetComponent<WokCookPhase>();

        // Guardar las posiciones originales de los bowls
        eggStartPosition = eggBowl.position;
        riceStartPosition = riceBowl.position;
    }
    public void ResetBowls()
    {
        eggBowl.gameObject.SetActive(true);
        riceBowl.gameObject.SetActive(true);

        eggBowl.position = eggStartPosition;
        riceBowl.position = riceStartPosition;
    }
    public void StartTransition()
    {
        StartCoroutine(Animations());
    }

    private IEnumerator Animations()
    {
        if (eggBowl == null || riceBowl == null) yield break;

        Vector3 eggStartPos = eggBowl.position;
        Vector3 riceStartPos = riceBowl.position;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);

            eggBowl.position = Vector3.Lerp(eggStartPos, eggHidePoint.position, t);
            riceBowl.position = Vector3.Lerp(riceStartPos, riceHidePoint.position, t);

            yield return null;
        }

        eggBowl.gameObject.SetActive(false);
        riceBowl.gameObject.SetActive(false);

        if (cookPhase != null) cookPhase.StartCooking();
    }
}