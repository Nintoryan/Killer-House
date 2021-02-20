using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FindThePair : Minigame
{
    [SerializeField] private List<PairItem> _items = new List<PairItem>();
    [SerializeField] private Sprite[] _sprites;
    
    [SerializeField] private UnityEvent _goalReached;

    private PairItem Selected;
    
    public event UnityAction GoalReached
    {
        add => _goalReached.AddListener(value);
        remove => _goalReached.RemoveListener(value);
    }

    private void Awake()
    {
        InitializeMiniGame();
    }

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();
        var list = new List<int>();
        var _tmp_items = _items;
        for (int i = 0; i < _sprites.Length;i++)
        {
            list.Add(i);
        }
        while (list.Count > 0)
        {
            var a = list[Random.Range(0, list.Count)];
            list.Remove(a);
            var item1 = _tmp_items[Random.Range(0, _tmp_items.Count)];
            _tmp_items.Remove(item1);
            var item2 = _tmp_items[Random.Range(0, _tmp_items.Count)];
            _tmp_items.Remove(item2);
            item1.Initialize(_sprites[a],a,this);
            item2.Initialize(_sprites[a],a,this);
        }
    }

    private int UnlockedAmount = 0;
    public void Touch(PairItem _pairItem)
    {
        if (!isMiniGameStarted)
        {
            base.StartMinigame();
        }
        if (Selected == null)
        {
            Selected = _pairItem;
            _pairItem.Open();
        }
        else
        {
            if (Selected == _pairItem) return;
            if (Selected.ID == _pairItem.ID)
            { 
                Selected.LockState();
                _pairItem.Open();
                _pairItem.LockState();
                Selected = null;
                UnlockedAmount++;
                if (UnlockedAmount == _sprites.Length)
                {
                    _goalReached?.Invoke();
                }
            }
            else
            {
                var s = DOTween.Sequence();
                s.AppendCallback(_pairItem.Open);
                s.AppendInterval(0.6f);
                s.AppendCallback(() =>
                {
                    Selected.Close();
                    _pairItem.Close();
                    Selected = null;
                });
            }
            
        }
    }
}
