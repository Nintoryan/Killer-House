using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeginEndGame : MonoBehaviour
{
    [SerializeField] private GameObject[] CharactersImages;
    [SerializeField] private GameObject Vinet;
    [SerializeField] private GameObject WholeScreen;
    [SerializeField] private Image DarkScreen;
    [SerializeField] private Image BG;
    [SerializeField] private Sprite GrayBg;
    [SerializeField] private Sprite GreenBg;
    [SerializeField] private GameObject CivilianScreen;
    [SerializeField] private GameObject KillerScreen;
    [SerializeField] private GameObject CivilianVictory;
    [SerializeField] private GameObject CivilianDefeat;
    [SerializeField] private GameObject ImposterVictory;

    public void SetCivilianVictory()
    {
        CivilianVictory.SetActive(true);
        CivilianScreen.SetActive(false);
        KillerScreen.SetActive(false);
    }

    public void TurnAllPortraitsOff()
    {
        foreach (var image in CharactersImages)
        {
            image.SetActive(false);
        }    
    }
    
    public void SetCivilianDefeat()
    {
        CivilianDefeat.SetActive(true);
        CivilianScreen.SetActive(false);
        KillerScreen.SetActive(false);
    }
    public void SetImposterVictory()
    {
        ImposterVictory.SetActive(true);
        CivilianScreen.SetActive(false);
        KillerScreen.SetActive(false);
    }
    public void FadeIn()
    {
        DarkScreen.gameObject.SetActive(true);
        DarkScreen.color = new Color(0,0,0,0);
        DarkScreen.DOFade(1,1);
    }

    public void FadeOut()
    {
        DarkScreen.color = new Color(0,0,0,1);
        DarkScreen.DOFade(0,1);
        var s = DOTween.Sequence();
        s.Append(DarkScreen.DOFade(0, 1));
        s.AppendCallback(() =>
        {
            DarkScreen.gameObject.SetActive(false);
        });
    }

    public void SetGreenBG()
    {
        BG.sprite = GreenBg;
    }

    public void SetGrayBG()
    {
        BG.sprite = GrayBg;
    }
    public void SetCharacterImageActive(int id,string Name)
    {
        CharactersImages[id].SetActive(true);
        CharactersImages[id].GetComponentInChildren<TMP_Text>().text = Name;
    }

    public void SetCharacteImageDead(int id)
    {
        CharactersImages[id].GetComponent<Image>().color = new Color(0.17f, 0.17f, 0.17f);
    }

    public void SetCharacterImageRed(int id)
    {
        CharactersImages[id].GetComponent<Image>().color = Color.red;
    }

    public void DisableScreen()
    {
        WholeScreen.SetActive(false);
    }

    public void SetCivilianScreen(bool isActive)
    {
        CivilianScreen.SetActive(isActive);
    }

    public void SetKillerScreen(bool isActive)
    {
        KillerScreen.SetActive(isActive);
        Vinet.SetActive(isActive);
    }

    public void ActivateScreen()
    {
        WholeScreen.SetActive(true);
    }
}
