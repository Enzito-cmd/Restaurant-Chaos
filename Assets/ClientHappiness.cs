using UnityEngine;
using UnityEngine.UI;

public class ClientHappiness : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image happinessFill;

    [Header("Time")]
    [SerializeField] private float maxTime = 90f;

    private float currentTime;
    private bool isRunning = true;

    private void Awake()
    {
        currentTime = maxTime;
        UpdateBar();

        // Empieza automáticamente desde que se crea el cliente
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;

            UpdateBar();

            Debug.Log("El cliente se quedó sin tiempo.");

            RestaurantClient client =
    GetComponent<RestaurantClient>();

            if (client != null)
            {
                client.Die();
            }

            return;
        }

        UpdateBar();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void UpdateBar()
    {
        if (happinessFill != null)
        {
            happinessFill.fillAmount = currentTime / maxTime;
        }
    }
}