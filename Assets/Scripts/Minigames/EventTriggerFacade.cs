using System;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggerFacade : MonoBehaviour
{
    public event UnityAction OnPointerDown;
    public event UnityAction OnPointerUp;

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            PointerUp();
        }
    }

    public void PointerDown()
    {
        OnPointerDown?.Invoke();
    }

    private void PointerUp()
    {
        OnPointerUp?.Invoke();
    }

    public void ClearEvents()
    {
        OnPointerDown = null;
        OnPointerUp = null;
    }
}
