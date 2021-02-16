using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DBDMinigame : Minigame
{
    [SerializeField] private Transform _arc;
    [SerializeField] private float[] _arcPositions;
    [SerializeField] private CursorRotator _cursor;
    [SerializeField] private Button _button;
    [SerializeField] private UnityEvent _goalReached;
    private int _index;

    public event UnityAction GoalReached
    {
        add => _goalReached.AddListener(value);
        remove => _goalReached.RemoveListener(value);
    }

    public override void StartMinigame()
    {
        base.StartMinigame();

        _index = 0;
        SetArcPosition(_index);

        _cursor.transform.DORotate(Vector3.forward * -359, _cursor.Step).SetLoops(-1).SetSpeedBased();
    }

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();

        _index = 0;
        SetArcPosition(_index);
        _cursor.enabled = true;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => OnTap());
    }

    public void OnTap()
    {
        if (!isMiniGameStarted)
        {
            StartMinigame();
        }

        var checkedAngle = 180 - _cursor.CurrentAngle;

        if (checkedAngle < _arcPositions[_index] && checkedAngle > (_arcPositions[_index] - 36))
        {
            _index++;

            if (_index < _arcPositions.Length)
            {
                SetArcPosition(_index);
            }
            else
            {
                _cursor.enabled = false;
                _goalReached?.Invoke();
            }
        }
    }

    private void SetArcPosition(int index)
    {
        _arc.rotation = Quaternion.Euler(Vector3.forward * _arcPositions[index]);
    }
}