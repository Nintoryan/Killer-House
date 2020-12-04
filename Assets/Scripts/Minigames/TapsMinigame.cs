using DG.Tweening;
using UnityEngine;

public class TapsMinigame : Minigame
{
    [SerializeField] private float DelayCountDown;
    [SerializeField] private RectTransform Button;
    [SerializeField] private Vector2[] Positons;
    [SerializeField] private GameObject AnimationScreen;
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject FailScreen;
    [SerializeField] private GameObject Guide;
    private float currentCountDown;
    private bool isMiniGameStarted;
    private int CurrentPositionID;

    public void InitializeMiniGame()
    {
        Guide.SetActive(true);
        FailScreen.SetActive(false);
        AnimationScreen.SetActive(true);
        Button.gameObject.SetActive(true);
        Button.localScale = Vector3.one;
        Button.anchoredPosition = Positons[0];
    }
    private void StartMinigame()
    {
        currentCountDown = DelayCountDown;
        isMiniGameStarted = true;
        CurrentPositionID = 0;
        Guide.SetActive(false);
    }
    private void Update()
    {
        if(!isMiniGameStarted) return;
        currentCountDown -= Time.deltaTime;
        if (currentCountDown < 0)
        {
            Fail();
        }
    }
    public void Tap()
    {
        if (!isMiniGameStarted)
        {
            StartMinigame();
        }
        currentCountDown = DelayCountDown;
        CurrentPositionID++;
        if (CurrentPositionID >= Positons.Length)
        {
            Win();
            return;
        }
        Button.localScale = Vector3.one;
        Button.DOKill();
        Button.DOScale(new Vector3(0, 0, 0), DelayCountDown);
        Button.DOAnchorPos(Positons[CurrentPositionID], 0.1f);
    }

    private void Win()
    {
        AnimationScreen.SetActive(false);
        WinScreen.SetActive(true);
        isMiniGameStarted = false;
        Button.gameObject.SetActive(false);
        Complete();
    }

    private void Fail()
    {
        AnimationScreen.SetActive(false);
        FailScreen.SetActive(true);
        isMiniGameStarted = false;
        Button.gameObject.SetActive(false);
    }
}
