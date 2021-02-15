using UnityEngine;
using UnityEngine.EventSystems;

public class RotatingInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private float _dumpingCofficient = 0.17f;

    public RotatingMinigame Rotating { get; set; }

    private void Start()
    {
        int defaultValue = EventSystem.current.pixelDragThreshold;
        EventSystem.current.pixelDragThreshold =
                Mathf.Max(
                     defaultValue,
                     (int)(defaultValue * Screen.dpi / 160f));
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        Rotating.TryToRotate(eventData.delta.magnitude * _dumpingCofficient);
    }

    public void OnEndDrag(PointerEventData eventData) { }
}