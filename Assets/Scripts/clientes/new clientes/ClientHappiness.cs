using UnityEngine;
using UnityEngine.UI;

public class ClientHappiness : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image happinessFill;
    [SerializeField] private GameObject happinessCanvas;

    [Header("tiempo necesario")]
    [SerializeField] private float maxTime = 700f;

    private float currentTime;

    public float CurrentTime => currentTime;
    public float MaxTime => maxTime;
    public float Percent => Mathf.Clamp01(currentTime / maxTime);
    public bool IsEmpty => currentTime <= 0;

    private void Awake()
    {
        currentTime = maxTime;
        UpdateBar();
    }

    private void Update()
    {
        UpdateBar();
    }

    public void Tick(float amount)
    {
        Debug.Log("ENTRÓ A TICK");

        currentTime -= amount * Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);

        Debug.Log("SALIÓ DE TICK");
    }

    public void Increase(float amount)
    {
        currentTime += amount;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);
        UpdateBar();
    }

    public void Decrease(float amount)
    {
        currentTime -= amount;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);
        UpdateBar();
    }

    public void ResetBar()
    {
        currentTime = maxTime;
        UpdateBar();
    }

    public void ShowBar()
    {
        if (happinessCanvas != null)
            happinessCanvas.SetActive(true);
    }

    public void HideBar()
    {
        if (happinessCanvas != null)
            happinessCanvas.SetActive(false);
    }

    private void UpdateBar()
    {
        if (happinessFill != null)
            happinessFill.fillAmount = Percent;
    }
}