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

    [Header("Cursors (hotspots are 0 to 1 from the top left)")]
    [SerializeField] private Texture2D normalTexture;
    [SerializeField] private Vector2 normalHotspot;
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
        Apply(normalTexture, normalHotspot);
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
                Apply(openHandTexture, openHandHotspot);
                break;
            case CursorStyle.ClosedHand:
                Apply(closedHandTexture, closedHandHotspot);
                break;
            default:
                Apply(normalTexture, normalHotspot);
                break;
        }
    }

    private static void Apply(Texture2D texture, Vector2 hotspot)
    {
        if (texture == null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        Cursor.SetCursor(texture, Vector2.Scale(hotspot, new Vector2(texture.width, texture.height)), CursorMode.Auto);
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
