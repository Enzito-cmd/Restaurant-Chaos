using UnityEngine;

public class TablePoint : MonoBehaviour
{
    public bool busy = false;
    [SerializeField] private GameObject free;

    private TableOrder tableOrder;

    [SerializeField] private int numberOfTable;

    public int _numberOfTable => numberOfTable;
    public TableOrder _tableOrder => tableOrder;

    public void Start()
    {
        tableOrder = GetComponentInParent<TableOrder>();
    }

    public void FreeIndicator(bool show)
    {
        if (free != null)
            free.SetActive(show && !busy);
    }
}

