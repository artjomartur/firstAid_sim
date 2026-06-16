using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Senior Developer Tool: WindowDragger allows retro dialog windows to be dragged 
/// by their title/header bars and brings the active window to the front.
/// </summary>
public class WindowDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform targetWindow;
    private Canvas canvas;
    private Vector2 pointerOffset;

    private void Start()
    {
        if (targetWindow == null)
        {
            targetWindow = transform.parent as RectTransform;
        }
        canvas = GetComponentInParent<Canvas>();

        // Ensure the drag handle can receive raycasts
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetWindow != null)
        {
            // Bring active window to front of the canvas rendering stack
            targetWindow.SetAsLastSibling();
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetWindow, eventData.position, eventData.pressEventCamera, out pointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null || targetWindow == null) return;

        RectTransform canvasRT = canvas.transform as RectTransform;
        Vector2 localPointerPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, eventData.pressEventCamera, out localPointerPos))
        {
            targetWindow.anchoredPosition = localPointerPos - pointerOffset;
        }
    }
}
