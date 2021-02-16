using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class FillingMinigame : Minigame
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _duration;
    [SerializeField] private UnityEvent _goalReached;
    [SerializeField] private EventTrigger _eventTrigger;
    [SerializeField] private EventTriggerFacade _eventTriggerFacade;
    private Coroutine _increasing;
    private float _step;

    private const float _startScale = 0.2f;

    public event UnityAction GoalReached
    {
        add => _goalReached.AddListener(value);
        remove => _goalReached.RemoveListener(value);
    }

    public override void StartMinigame()
    {
        base.StartMinigame();

        _step = (1 - _startScale) / _duration;
    }

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();
        _target.localScale = Vector3.one * _startScale;
        _eventTriggerFacade.ClearEvents();
        _eventTriggerFacade.OnPointerDown += Launch;
        _eventTriggerFacade.OnPointerUp += Stop;
    }

    public void Launch()
    {
        _increasing = StartCoroutine(Increasing());

        if (isMiniGameStarted)
        {
            StartMinigame();
        }
    }

    public void Stop()
    {
        if (_increasing != null)
        {
            StopCoroutine(_increasing);
        }
    }

    private IEnumerator Increasing()
    {
        var currentScale = _target.localScale.x;

        while (currentScale <= 1)
        {
            _target.localScale += Vector3.one * _step * Time.fixedDeltaTime;
            currentScale = _target.localScale.x;

            yield return new WaitForFixedUpdate();
        }

        _goalReached?.Invoke();
    }
}