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
        if (basketUpPoint != null) targetBasketPos = basketUpPoint.position;
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

