using UnityEngine;

public class PreviewerRotation : MonoBehaviour 
{
    [SerializeField] private float _sensitivity;
    private Vector3 _mouseReference;
    private Vector3 _mouseOffset;
    private Vector3 _rotation;
    private bool _isRotating;

    private void Start ()
    {
        _rotation = Vector3.zero;
    }

    private void Update()
    {
        if (!_isRotating) return;
        
        _mouseOffset = (Input.mousePosition - _mouseReference);
        _rotation.y = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;
        transform.Rotate(_rotation);
        _mouseReference = Input.mousePosition;
    }
     
    public void MouseDown()
    {
        _isRotating = true;
        _mouseReference = Input.mousePosition;
    }
     
    public void MouseUp()
    {
        _isRotating = false;
    }
     
}