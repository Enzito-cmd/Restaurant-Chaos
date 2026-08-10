using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClientSpawner : MonoBehaviour
{
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int maxClients = 50;
    [SerializeField] private float spacing = 1.5f;
    [Header("Espera de aparicion")]
    [SerializeField] private float wait = 30f;
    [SerializeField] private float speed = 3f;
    private List<GameObject> clients = new List<GameObject>();

    [Header("Sounds Section")]
    [SerializeField] private AudioClip entranceSound;
    private SoundManager soundManager;

    void Start()
    {
        soundManager = GameManager.instance._soundManager;

        StartCoroutine(SpawnClients());
    }
    void Update()
    {
        Move();
    }
    void Awake()
    {
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
    private ClientData FindData(string name)
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:ClientData " + name, new[] { "Assets/Scripts/new clientes" });

        if (guids.Length > 0)
            return AssetDatabase.LoadAssetAtPath<ClientData>(AssetDatabase.GUIDToAssetPath(guids[0]));
#endif

        Debug.LogError("No se encontró ClientData: " + name);
        return null;
    }
    IEnumerator SpawnClients()
    {
        int i = 0;

        while (true)
        {
            clients.RemoveAll(c => c == null);

            if (clients.Count < maxClients)
            {
                Vector3 pos = spawnPoint.position -
                              new Vector3(0, 0, clients.Count * spacing);

                ClientData selectedData;

                if (i == 2)
                    selectedData = FindData("Shakuza");
                else if (i == 4)
                    selectedData = FindData("OldWoman");
                else
                    selectedData = FindData("Normal");

                GameObject client = Instantiate(
                    clientPrefab,
                    pos,
                    Quaternion.identity
                );

                ClientBehaviour cb = client.GetComponent<ClientBehaviour>();

                cb.SetSpawner(this);
                cb.SetData(selectedData);

                clients.Add(client);

                i++;
            }

            soundManager.ReproduceSound(entranceSound);

            yield return new WaitForSeconds(wait);
        }
    }
    void Move()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i] == null)
            {
                continue;
            }

            ClientBehaviour cb = clients[i].GetComponent<ClientBehaviour>();

            if (cb != null && (cb.IsBroken || cb.IsAngry))
            {
                continue;
            }
            Vector3 line = spawnPoint.position - new Vector3(0, 0, i * spacing);//hace la fia
            //
            float moveSpeed = speed;

            if (cb != null)
            {
                moveSpeed = cb.GetQueueSpeed();
            }

            clients[i].transform.position = Vector3.Lerp(clients[i].transform.position,line, moveSpeed * Time.deltaTime);
        }
    }
    public bool HasClients()
    {
        return clients.Count > 0;//hay 1 cliente en la fila?
    }

    public GameObject GetFirstClient()
    {
        if (clients.Count > 0)
        {
            return clients[0];//primero de la fila
        }
        return null;//no hay ninguno
    }
    public void RemoveClient(GameObject client)
    {
        if (clients.Contains(client))
        {
            clients.Remove(client);
        }
    }

    public void RemoveFirstClient()
    {
        if (clients.Count > 0)
            clients.RemoveAt(0);
    }
}