using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // Read by PlayerController. X = left/right, Y = forward/back, range -1 to 1.
    public static Vector2 Direction { get; private set; }

    public RectTransform knob;
    RectTransform bg;
    float radius;

    void Awake()
    {
        bg = (RectTransform)transform;
        radius = bg.sizeDelta.x * 0.5f;
    }

    public void OnPointerDown(PointerEventData e) { OnDrag(e); }

    public void OnDrag(PointerEventData e)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(bg, e.position, e.pressEventCamera, out local);
        Vector2 v = Vector2.ClampMagnitude(local / radius, 1f);
        Direction = v;
        if (knob != null) knob.anchoredPosition = v * radius;
    }

    public void OnPointerUp(PointerEventData e)
    {
        Direction = Vector2.zero;
        if (knob != null) knob.anchoredPosition = Vector2.zero;
    }
}