using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BeginEndGame : MonoBehaviour
{
    [SerializeField] private GameObject[] CharactersImages;
    [SerializeField] private GameObject CivilianScreen;
    [SerializeField] private GameObject KillerScreen;
    [SerializeField] private GameObject Vinet;
    [SerializeField] private GameObject WholeScreen;
    [SerializeField] private Image DarkScreen;

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

    public void SetCharacterImageActive(int id)
    {
        CharactersImages[id].SetActive(true);
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
