using UnityEngine;

public enum CursorStyle
{
    Default,
    OpenHand,
    ClosedHand
}

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Hand Cursors")]
    [SerializeField] private Texture2D openHandTexture;
    [SerializeField] private Vector2 openHandHotspot;
    [SerializeField] private Texture2D closedHandTexture;
    [SerializeField] private Vector2 closedHandHotspot;

    private CursorStyle currentStyle = CursorStyle.Default;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        HideCursor();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            Instance = null;
        }
    }

    public static void Request(CursorStyle style)
    {
        if (Instance != null)
        {
            Instance.SetStyle(style);
        }
    }

    public void SetStyle(CursorStyle style)
    {
        if (style == currentStyle)
        {
            return;
        }

        currentStyle = style;

        switch (style)
        {
            case CursorStyle.OpenHand:
                Cursor.SetCursor(openHandTexture, openHandHotspot, CursorMode.Auto);
                break;
            case CursorStyle.ClosedHand:
                Cursor.SetCursor(closedHandTexture, closedHandHotspot, CursorMode.Auto);
                break;
            default:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;
        }
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideCursor()
    {
        SetStyle(CursorStyle.Default);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
