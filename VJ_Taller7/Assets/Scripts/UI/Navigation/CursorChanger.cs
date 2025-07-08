using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    private Vector2 hotspot = Vector2.zero;
    private CursorMode cursorMode = CursorMode.Auto;

    private void Start()
    {
        ChangeCursor(cursorTexture);
    }

    public void ChangeCursor(Texture2D newCursor)
    {
        Cursor.SetCursor(newCursor, hotspot, cursorMode);
    }
}
