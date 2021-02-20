using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    [SerializeField] private RectTransform Movable;
    [SerializeField] private RectTransform Key;
    [SerializeField] private RectTransform Skull;
    [SerializeField] private RectTransform Glowing;
    [SerializeField] private Button ClickButton;
    [SerializeField] private MoneyBar _moneyBar;
    [Header("Flyings")]
    [SerializeField] private RectTransform FlyingSkullPrefab;
    [SerializeField] private RectTransform FlyingKeyPrefab;
    [SerializeField] private RectTransform FlyingParent;
    [SerializeField] private RectTransform MB_keys;
    [SerializeField] private RectTransform MB_skulls;
    
    [SerializeField] private TMP_Text Result;

    private void Start()
    {
        var amountOfSkulls = Random.Range(50, 150);
        
        var s = DOTween.Sequence();
        s.Append(Movable.DOAnchorPosY(0, 1.25f).SetEase(Ease.InOutBack));
        s.AppendCallback(() => { Result.text = $"+{amountOfSkulls} skulls"; });
        s.AppendInterval(0.5f);
        var list = new List<RectTransform>();
        for (int i = 0; i < 25; i++)
        {
            var a = Instantiate(FlyingSkullPrefab, FlyingParent);
            list.Add(a);
            a.gameObject.SetActive(false);
            s.AppendCallback(() =>
            {
                a.anchoredPosition += new Vector2(Random.Range(-100, 100), Random.Range(-100, 100));
                a.gameObject.SetActive(true);
            });
            s.AppendInterval(0.04f);
        }

        for (int i = 0; i < 25; i++)
        {
            s.Join(list[i].DOMove(MB_skulls.transform.position, 0.6f).SetEase(Ease.InOutQuart));
        }

        for (int i = 0; i < 25; i++)
        {
            s.Append(list[i].GetComponent<Image>().DOFade(0, 0.03f));
        }

        s.AppendCallback(() =>
        {
            UserData.Wallet.Balance += amountOfSkulls;
            _moneyBar.Refresh();
        });
        s.AppendInterval(0.1f);
        s.AppendCallback(() =>
        {
            ClickButton.interactable = true;
        });
        //Тут клик
    }

    private bool isButtonClicked;
    
    public void ButtonClick()
    {
        if (!isButtonClicked )
        {
            ClickFirst();
            isButtonClicked = true;
        }
        else
        {
            ClickSecond();
        }
    }
    
    private void ClickFirst()
    {
        var s = DOTween.Sequence();
        s.AppendCallback(() =>
        {
            ClickButton.interactable = false;
        });
        s.Append(Glowing.DOScale(Vector3.zero, 0.5f));
        s.Join(Skull.DOScale(Vector3.zero, 0.5f));
        s.Join(Result.DOFade(0, 0.5f));

        s.AppendInterval(0.5f);

        s.AppendCallback(() =>
        {
            Key.transform.localScale = new Vector3(0,0,0);
            Key.gameObject.SetActive(true);
            Result.text = "";
            Skull.gameObject.SetActive(false);
        });
        s.Append(Glowing.DOScale(Vector3.one, 0.5f));
        s.Join(Key.DOScale(Vector3.one, 0.5f));
        s.Join(Result.DOFade(1, 0.5f));
        s.AppendCallback(() =>
        {
            Result.text = "+1 key";
        });
        s.AppendInterval(0.5f);
        var a = Instantiate(FlyingKeyPrefab, FlyingParent);
        a.gameObject.SetActive(false);
        s.AppendCallback(() =>
        {
            a.gameObject.SetActive(true);
        });
        s.AppendInterval(0.04f);
        s.Join(a.DOMove(MB_keys.transform.position, 0.6f).SetEase(Ease.InOutQuart));
        s.Append(a.GetComponent<Image>().DOFade(0, 0.03f));
        s.AppendCallback(() =>
        {
            UserData.Wallet.Keys += 1;
            _moneyBar.Refresh();
        });
        s.AppendInterval(0.1f);
        s.AppendCallback(() =>
        {
            ClickButton.interactable = true;
        });
    }

    private void ClickSecond()
    {
        var s = DOTween.Sequence();
        s.AppendCallback(() =>
        {
            ClickButton.interactable = false;
        });
        s.Append(Glowing.DOScale(Vector3.zero, 0.5f));
        s.Join(Key.DOScale(Vector3.zero, 0.5f));
        s.Join(Result.DOFade(0, 0.5f));
        s.AppendInterval(1.5f);
        s.AppendCallback(() =>
        {
            if (Advertisment.Instance != null)
            {
                if (Advertisment.Instance.IsInterstitialReady && PlayerPrefs.GetInt("NoAds") != 1)
                {
                    Advertisment.Instance.ShowInterstitial();
                }
            }
        });
        s.AppendInterval(1f);
        s.AppendCallback(() =>
        {
            SceneManager.LoadScene("LoginMenuHub");
        });
    }
}
