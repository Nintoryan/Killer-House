using UnityEngine;
using UnityEngine.Events;

public class RotatingMinigame : Minigame
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _startAngle;
    [SerializeField] private RotationDirection _direction;
    [SerializeField] private int _neededTurnoverAmount;
    [SerializeField] private RotatingInput _rotatingInput;
    [SerializeField] private GameObject _leftArrow;
    [SerializeField] private GameObject _rightArrow;
    [Space, SerializeField] private UnityEvent _goalReached;
    private int _turnoverAmount;
    private float _radius;
    private float _currentAngle;

    public event UnityAction GoalReached
    {
        add => _goalReached.AddListener(value);
        remove => _goalReached.RemoveListener(value);
    }

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();

        _radius = Vector3.Magnitude(_target.localPosition);
        var newDirection = RotatingUtilities.CalculatePosition(_startAngle * Mathf.Deg2Rad);

        _target.localPosition = newDirection * _radius;
        _currentAngle = _startAngle;

        _rotatingInput.Rotating = this;
        _turnoverAmount = 0;

        if (_direction == RotationDirection.Clockwise)
        {
            _rightArrow.SetActive(true);
            _leftArrow.SetActive(false);
        }
        else
        {
            _rightArrow.SetActive(false);
            _leftArrow.SetActive(true);
        }
    }

    public void TryToRotate(float angle)
    {
        if (_direction == RotationDirection.Clockwise)
        {
            CheckClockwiseTurnover();
        }
        else
        {
            angle *= -1;
            CheckCounterclockWiseTurnover();
        }

        if (isMiniGameStarted == false)
        {
            StartMinigame();
        }

        _target.localPosition = RotatingUtilities.Rotate(ref _currentAngle, angle) * _radius;
    }

    private void CheckClockwiseTurnover()
    {
        var nextTurnoverAngle = _startAngle + (360 * (_turnoverAmount + 1));

        if (_currentAngle > nextTurnoverAngle)
        {
            _turnoverAmount++;

            CheckGoal();
        }
    }

    private void CheckCounterclockWiseTurnover()
    {
        var nextTurnoverAngle = _startAngle - (360 * (_turnoverAmount + 1));

        if (_currentAngle < nextTurnoverAngle)
        {
            _turnoverAmount++;

            CheckGoal();
        }
    }

    private void CheckGoal()
    {
        if (_turnoverAmount >= _neededTurnoverAmount)
        {
            _goalReached?.Invoke();
        }
    }
}