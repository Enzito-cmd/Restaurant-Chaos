using UnityEngine;

[CreateAssetMenu(fileName = "ClientData", menuName = "Restaurant/Client Data")]
public class ClientData : ScriptableObject
{
    [Header("General")]
    public string clientName;

    public ClientBehaviour.ClientType clientType;

    [Header("Movement")]
    public float normalSpeed = 2;
    public float queueSpeed = 3;
    public float floorSpeed = 6;

    [Header("Behaviour")]
    public bool followsPlayer;
    public bool canBecomeAngry;
    public bool canBreakDishes;

    [Header("Timers")]
    public int minThinkingTime = 2;
    public int maxThinkingTime = 5;
    public float eatingTime = 10;

    [Header("Happiness")]
    public float happinessMultiplier = 1;

    [Header("Follow")]
    public float followMultiplier = 1;

    [Header("Food")]
    public TypeOfFoods[] foods;

    [Header("Visual")]
    public GameObject modelPrefab;
}