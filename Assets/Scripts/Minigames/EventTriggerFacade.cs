using UnityEngine;
using UnityEngine.Events;

public class EventTriggerFacade : MonoBehaviour
{
    public event UnityAction OnPointerDown;
    public event UnityAction OnPointerUp;

    public void PointerDown()
    {
        OnPointerDown?.Invoke();
    }

    public void PointerUp()
    {
        OnPointerUp?.Invoke();
    }

    public void ClearEvents()
    {
        OnPointerDown = null;
        OnPointerUp = null;
    }
}
