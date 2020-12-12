using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TapsMinigame : Minigame
{
    [SerializeField] private float DelayCountDown;
    [SerializeField] private RectTransform Button;
    [SerializeField] private Vector2[] Positons;
    private int CurrentPositionID;

    public override void InitializeMiniGame()
    {
        base.InitializeMiniGame();
        Button.gameObject.SetActive(true);
        Button.GetComponent<Button>().onClick.AddListener(Tap);
        Button.localScale = Vector3.one;
        Button.anchoredPosition = Positons[0];
    }

    public override void StartMinigame()
    {
        base.StartMinigame();
        CurrentPositionID = 0;
    }

    private void Tap()
    {
        if (!isMiniGameStarted)
        {
            StartMinigame();
        }
        _minigamesManager.PlaySound(CurrentPositionID);
        CurrentPositionID++;
        if (CurrentPositionID >= Positons.Length)
        {
            Win();
            Button.gameObject.SetActive(false);
            Button.GetComponent<Button>().onClick.RemoveAllListeners();
            return;
        }
        Button.localScale = Vector3.one;
        Button.DOKill();
        Button.DOScale(new Vector3(0.5f, 0.5f, 0.5f), DelayCountDown);
        Button.DOAnchorPos(Positons[CurrentPositionID], 0.1f);
    }
}
