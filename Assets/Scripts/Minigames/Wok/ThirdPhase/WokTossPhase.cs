using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WokTossPhase : MonoBehaviour
{
    [Header("Skillcheck")]
    [SerializeField] private Slider slider;

    [Header("Rice")]
    [SerializeField] private Transform rice;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float airTime = 1f;

    [Header("Plate")]
    [SerializeField] private Transform plate;
    [SerializeField] private Transform plateRicePoint;
    [SerializeField] private Transform plateStartPoint;
    [SerializeField] private Transform plateCatchPoint;
    [SerializeField] private Transform plateEndPoint;

    private Transform riceParent;
    private Vector3 ricePosition;
    private Quaternion riceRotation;
    private Vector3 riceScale;

    private void Awake()
    {
        if (rice == null) return;

        riceParent = rice.parent;
        ricePosition = rice.localPosition;
        riceRotation = rice.localRotation;
        riceScale = rice.localScale;
    }

    public void Begin()
    {
        if (slider != null)
        {
            slider.value = 0f;
            slider.gameObject.SetActive(true);
        }

        if (rice != null)
        {
            rice.gameObject.SetActive(true);
        }

        SoundManager.Instance?.PlaySound(SoundType.WokFinalSkillCheck);
    }

    public void SetProgress(float progress)
    {
        if (slider != null)
        {
            slider.value = progress;
        }
    }

    public IEnumerator Play()
    {
        if (slider != null)
        {
            slider.gameObject.SetActive(false);
        }

        if (rice == null || plate == null || plateRicePoint == null || plateStartPoint == null || plateCatchPoint == null || plateEndPoint == null)
        {
            yield break;
        }

        rice.SetParent(null, true);
        plate.position = plateStartPoint.position;
        plate.gameObject.SetActive(true);

        Coroutine jump = StartCoroutine(MinigameMotion.Arc(rice, () => plateRicePoint.position, airTime, jumpHeight));

        yield return MinigameMotion.Slide(plate, plateCatchPoint.position, airTime);
        yield return jump;

        rice.SetParent(plateRicePoint, true);

        float approach = Mathf.Max(Vector3.Distance(plateStartPoint.position, plateCatchPoint.position), 0.01f);
        float exit = Vector3.Distance(plateCatchPoint.position, plateEndPoint.position);

        yield return MinigameMotion.Slide(plate, plateEndPoint.position, airTime * exit / approach);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        if (slider != null)
        {
            slider.gameObject.SetActive(false);
        }

        if (plate != null)
        {
            plate.gameObject.SetActive(false);
        }

        if (rice == null) return;

        rice.SetParent(riceParent, false);
        rice.localPosition = ricePosition;
        rice.localRotation = riceRotation;
        rice.localScale = riceScale;
        rice.gameObject.SetActive(false);
    }
}
