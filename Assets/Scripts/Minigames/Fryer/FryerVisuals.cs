using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class FryerVisuals : MonoBehaviour
{
    [Header("Basket References")]
    public Transform basket;
    public Transform basketUpPoint;
    public Transform basketDownPoint;
    public float basketSpeed = 3f;
    private Vector3 targetBasketPos;

    [Header("Tempuras Physics")]
    public GameObject tempuraPrefab;
    public Transform[] spawnPoints;
    public float bumpForce = 2f;
    public float torqueForce = 1f;

    private List<Rigidbody> spawnedTempuras = new List<Rigidbody>();

    [Header("Serve Animation")]
    public Transform plate;
    public Transform plateFoodPoint;
    public Transform plateStartPoint;
    public Transform plateStopPoint;
    public Transform plateEndPoint;
    public float plateMoveDuration = 0.8f;
    public Vector3 basketTiltAxis = Vector3.forward;
    public float basketTiltAngle = -70f;
    public float basketTiltDuration = 0.35f;
    public float tempuraFlightTime = 0.6f;
    public float tempuraArcHeight = 1f;
    public float tempuraDelay = 0.15f;
    public float tempuraSpacing = 0.5f;

    [Header("Skillcheck UI")]
    public GameObject skillcheckCanvas;
    public RectTransform needle;
    public Image successZone;
    public TextMeshProUGUI countdownText;

    [Header("Counters UI")]
    public TextMeshProUGUI hitsText;
    public TextMeshProUGUI missesText;

    private void Start()
    {
        if (basket != null) targetBasketPos = basket.position;
        if (skillcheckCanvas != null) skillcheckCanvas.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        UpdateHits(0, 0);
        UpdateMisses(0, 0);
    }

    private void Update()
    {
        if (basket != null)
        {
            basket.position = Vector3.Lerp(basket.position, targetBasketPos, Time.deltaTime * basketSpeed);
        }
    }

    public void SpawnTempuras()
    {
        ClearTempuras();

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;
            Quaternion randomRotation = Random.rotation;

            GameObject temp = Instantiate(tempuraPrefab, spawnPoint.position, randomRotation);
            if (temp.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                spawnedTempuras.Add(rb);
            }
        }
    }

    public void MoveBasketDown()
    {
        if (basketDownPoint != null) targetBasketPos = basketDownPoint.position;
        WakeUpTempuras();
    }

    public void MoveBasketUp()
    {
        if (basketUpPoint != null) targetBasketPos = basketUpPoint.position;
        WakeUpTempuras();
    }

    public IEnumerator ServeTempuras()
    {
        if (basket == null || plate == null || plateFoodPoint == null || plateStartPoint == null || plateStopPoint == null || plateEndPoint == null) yield break;

        plate.position = plateStartPoint.position;
        plate.gameObject.SetActive(true);

        foreach (Rigidbody tempura in spawnedTempuras)
        {
            if (tempura == null) continue;

            tempura.isKinematic = true;
            tempura.detectCollisions = false;
            tempura.transform.SetParent(basket, true);
        }

        yield return MinigameMotion.Move(plate, plateStopPoint.position, plateMoveDuration);

        Quaternion rest = basket.rotation;

        StartCoroutine(TiltBasket(rest, Quaternion.AngleAxis(basketTiltAngle, basketTiltAxis) * rest));

        yield return new WaitForSeconds(basketTiltDuration * 0.5f);

        Coroutine last = null;
        float center = (spawnedTempuras.Count - 1) * 0.5f;

        for (int i = 0; i < spawnedTempuras.Count; i++)
        {
            if (spawnedTempuras[i] == null) continue;

            Vector3 landing = plateFoodPoint.position + plateFoodPoint.right * ((i - center) * tempuraSpacing);

            last = StartCoroutine(ThrowTempura(spawnedTempuras[i].transform, landing));

            yield return new WaitForSeconds(tempuraDelay);
        }

        yield return last;

        StartCoroutine(TiltBasket(basket.rotation, rest));

        yield return MinigameMotion.Move(plate, plateEndPoint.position, plateMoveDuration);

        plate.gameObject.SetActive(false);
    }

    private IEnumerator ThrowTempura(Transform tempura, Vector3 landing)
    {
        tempura.SetParent(null, true);

        yield return MinigameMotion.Arc(tempura, () => landing, tempuraFlightTime, tempuraArcHeight);

        tempura.SetParent(plateFoodPoint, true);
    }

    private IEnumerator TiltBasket(Quaternion from, Quaternion to)
    {
        for (float t = 0f; t < 1f; t += Time.deltaTime / basketTiltDuration)
        {
            basket.rotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        basket.rotation = to;
    }

    private void WakeUpTempuras()
    {
        foreach (Rigidbody rb in spawnedTempuras)
        {
            if (rb != null)
            {
                rb.WakeUp();
            }
        }
    }

    public void BumpTempuras()
    {
        foreach (Rigidbody rb in spawnedTempuras)
        {
            if (rb != null)
            {
                rb.AddForce(Vector3.up * bumpForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
            }
        }
    }

    public void UpdateSkillcheckUI(float needleAngle, float zoneStartAngle, float zoneFillAmount)
    {
        if (needle != null)
        {
            needle.localRotation = Quaternion.Euler(0, 0, -needleAngle);
        }

        if (successZone != null)
        {
            successZone.rectTransform.localRotation = Quaternion.Euler(0, 0, -zoneStartAngle);
            successZone.fillAmount = zoneFillAmount;
        }
    }

    public void ShowSkillcheck(bool show)
    {
        if (skillcheckCanvas != null) skillcheckCanvas.SetActive(show);
    }

    public void UpdateCountdownText(string text)
    {
        if (countdownText != null) countdownText.text = text;
    }

    public void ShowCountdown(bool show)
    {
        if (countdownText != null) countdownText.gameObject.SetActive(show);
    }


    public void UpdateHits(int currentHits, int requiredHits)
    {
        if (hitsText != null)
        {
            hitsText.text = $"{currentHits}/{requiredHits}";
        }
    }

    public void UpdateMisses(int currentMisses, int maxMisses)
    {
        if (missesText != null)
        {
            missesText.text = $"{currentMisses}/{maxMisses}";
        }
    }

    public void ClearTempuras()
    {
        foreach (Rigidbody rb in spawnedTempuras)
        {
            if (rb != null) Destroy(rb.gameObject);
        }
        spawnedTempuras.Clear();
    }
}