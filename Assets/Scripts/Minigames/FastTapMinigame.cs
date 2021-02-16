using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class FastTapMinigame : Minigame
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _increasingStep;
    [SerializeField] private float _decreasingStep;
    [SerializeField] private Button _button;
    [SerializeField] private UnityEvent _goalReached;
    private float _minScale = 0.2f;
    private bool _isDone;

    public event UnityAction GoalReached
    {
        add => _goalReached.AddListener(value);
        remove => _goalReached.RemoveListener(value);
    }

    private void FixedUpdate()
    {
        if (_target.localScale.x >= _minScale && _isDone == false)
        {
            _target.localScale -= Vector3.one * _decreasingStep * Time.fixedDeltaTime;
        }
    }

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();

        _target.localScale = Vector3.one * _minScale;
        _isDone = false;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => OnTap());
    }

    public void OnTap()
    {
        if (!isMiniGameStarted)
        {
            StartMinigame();
        }

        if (_target.localScale.x <= 1)
        {
            _target.localScale += Vector3.one * _increasingStep;
        }
        else
        {
            _isDone = true;
            _goalReached?.Invoke();
        }
    }
}