using UnityEngine;

public class CursorRotator : MonoBehaviour
{
    [SerializeField] private Transform _cursor;
    [SerializeField] private float _step;
    private float _radius;
    private float _currentAngle;

    public float Step => _step;

    public float CurrentAngle => _currentAngle;

    private void Start()
    {
        _radius = Vector3.Magnitude(_cursor.localPosition);
    }

    private void FixedUpdate()
    {
        _cursor.localPosition = RotatingUtilities.Rotate(ref _currentAngle, _step * Time.fixedDeltaTime) * _radius;
        TryToResetAngle();
    }

    private void TryToResetAngle()
    {
        if (_currentAngle >= 360)
        {
            _currentAngle = 0;
        }
    }
}