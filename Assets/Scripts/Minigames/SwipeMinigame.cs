using System;
using DG.Tweening;
using UnityEngine;

public class SwipeMinigame : Minigame
{
    [SerializeField] private Swipe _swipeControlls;
    [SerializeField] private RectTransform GuidePointer;
    [SerializeField] private Vector2 BottomGuidePos;
    [SerializeField] private Vector2 TopGuidePos;
    [SerializeField] private Vector2 LeftGuidePos;
    [SerializeField] private Vector2 RightGuidePos;
    [SerializeField] private Direction[] _directions;
    [SerializeField] private float DelayCountDown;
    private float currentCountDown;
    private int CurrentPositionID;

    private bool isPlaying;

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();
        GuidePointer.gameObject.SetActive(true);
        currentCountDown = DelayCountDown;
        isPlaying = true;
        ShowWhereHaveToSwipe(_directions[0]);
    }

    public override void StartMinigame()
    {
        base.StartMinigame();
        currentCountDown = DelayCountDown;
        CurrentPositionID = 0;
    }
    
    private void Update()
    {
        if(!isPlaying) return;
        if (_swipeControlls.SwipeDown)
        {
            DoSwipe(Direction.Down);
        }
        else if (_swipeControlls.SwipeUp)
        {
            DoSwipe(Direction.Up);
        }
        else if (_swipeControlls.SwipeLeft)
        {
            DoSwipe(Direction.Left);
        }
        else if (_swipeControlls.SwipeRight)
        {
            DoSwipe(Direction.Right);
        }
        if(!isMiniGameStarted) return;
        currentCountDown -= Time.deltaTime;
        if (currentCountDown < 0)
        {
            Debug.Log($"isMinigameStarted:{isMiniGameStarted}");
            Debug.Log($"currentCountDown:{currentCountDown}");
            Debug.Log($"gameObject.name:{gameObject.name}");
            isPlaying = false;
            Fail();
            GuidePointer.gameObject.SetActive(false);
        }
    }
    private void DoSwipe(Direction userinput)
    {
        Debug.Log("Do swipe performed!");
        if(!isMiniGameStarted)
        {
            StartMinigame();
        }
        if (userinput == _directions[CurrentPositionID])
        {
            currentCountDown = DelayCountDown;
            CurrentPositionID++;
            if (CurrentPositionID >= _directions.Length)
            {
                isPlaying = false;
                Win();
                currentCountDown = 100000;
                GuidePointer.gameObject.SetActive(false);
            }
            else
            {
                ShowWhereHaveToSwipe(_directions[CurrentPositionID]);
            }
        }
    }

    private void ShowWhereHaveToSwipe(Direction direction)
    {
        GuidePointer.DOKill();
        var s = DOTween.Sequence();
        s.SetLoops(180, LoopType.Restart);
        switch (direction)
        {
            case Direction.Up:
                s.AppendCallback(() => { GuidePointer.anchoredPosition = BottomGuidePos; });
                s.Append(GuidePointer.DOAnchorPos(TopGuidePos, DelayCountDown/3));
                break;
            case Direction.Down:
                s.AppendCallback(() => { GuidePointer.anchoredPosition = TopGuidePos; });
                s.Append(GuidePointer.DOAnchorPos(BottomGuidePos, DelayCountDown/3));
                break;
            case Direction.Left:
                s.AppendCallback(() => { GuidePointer.anchoredPosition = RightGuidePos; });
                s.Append(GuidePointer.DOAnchorPos(LeftGuidePos, DelayCountDown/3));
                break;
            case Direction.Right:
                s.AppendCallback(() => { GuidePointer.anchoredPosition = LeftGuidePos; });
                s.Append(GuidePointer.DOAnchorPos(RightGuidePos, DelayCountDown/3));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
}

[Serializable]
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}
