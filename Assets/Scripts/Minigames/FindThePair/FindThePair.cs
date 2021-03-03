using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class FindThePair : Minigame
{
    [SerializeField] private List<PairItem> _items;
    [SerializeField] private Sprite[] _sprites;
    
    [SerializeField] private UnityEvent _goalReached;

    private PairItem Selected;
    
    public event UnityAction GoalReached
    {
        add => _goalReached.AddListener(value);
        remove => _goalReached.RemoveListener(value);
    }

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();
        var list = new List<int>();
        var _tmp_items = _items.ToList();
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
                _pairItem.Open();
                Selected.LockState();
                _pairItem.LockState();
                UnlockedAmount++;
                if (UnlockedAmount == _sprites.Length)
                {
                    _goalReached?.Invoke();
                }
            }
            else
            {
                var s1 = Selected;
                var s2 = _pairItem;
                var s = DOTween.Sequence();
                s.AppendCallback(_pairItem.Open);
                s.AppendInterval(0.6f);
                s.AppendCallback(() =>
                {
                    if (s1 != Selected)
                    {
                        s1.Close();
                    }

                    if (s2 != Selected)
                    {
                        s2.Close();
                    }
                });
            }
            Selected = null;
        }
    }
}
