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
    private Coroutine _increasing;
    private float _step;
    private float _startScale;

    public event UnityAction GoalReached
    {
        add => _goalReached.AddListener(value);
        remove => _goalReached.RemoveListener(value);
    }

    private void Start()
    {
        _step = (1 - _target.localScale.x) / _duration;
        _startScale = _target.localScale.x;
    }

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();
        _target.localScale = Vector3.one * _startScale;

        _eventTrigger.triggers.Clear();

        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((call) => { Launch(); });

        EventTrigger.Entry entry1 = new EventTrigger.Entry();

        entry1.eventID = EventTriggerType.PointerUp;
        entry1.callback.AddListener((call) => { Stop(); });
        
        _eventTrigger.triggers.Add(entry);
        _eventTrigger.triggers.Add(entry1);
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