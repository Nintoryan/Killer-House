using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeginEndGame : MonoBehaviour
{
    [SerializeField] private GameObject[] Characters;
    [SerializeField] private GameObject CharacterRawImage;
    [SerializeField] private TMP_Text CivilianNickName;
    [SerializeField] private TMP_Text CivilianStartSubTitle;
    [SerializeField] private GameObject[] Killers;
    [SerializeField] private TMP_Text[] KillersNickNames;
    [SerializeField] private TMP_Text KillerStartTitle;
    [SerializeField] private GameObject WholeScreen;
    [SerializeField] private Image DarkScreen;
    [SerializeField] private Image BG;
    [SerializeField] private Sprite GrayBg;
    [SerializeField] private Sprite GreenBg;
    [SerializeField] private GameObject CivilianScreen;
    [SerializeField] private GameObject KillerScreen;
    [SerializeField] private GameObject CivilianVictory;
    [SerializeField] private GameObject ImposterVictory;
    [Header("Sounds")]
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _StartSound;
    [SerializeField] private AudioClip _CivilianWinSound;
    [SerializeField] private AudioClip _ImposterWinSound;
    
    private void Start()
    {
        StartCoroutine(FindAudioSource());
    }
    
    public void StartCivilianScreen()
    {
        BG.sprite = GreenBg;
        CivilianScreen.SetActive(true);
        _audioSource.PlayOneShot(_StartSound);
        CharacterRawImage.SetActive(true);
        Characters[GameManager.Instance.LocalPlayer.SkinID].SetActive(true);
        CivilianNickName.text = GameManager.Instance.LocalPlayer.Name;
        CivilianStartSubTitle.text =
            $"there is <color=red>{GameManager.Instance.KillerPlayers.Count} killer</color> among us";
    }

    public void StartKillerScreen()
    {
        BG.sprite = GrayBg;
        KillerScreen.SetActive(true);
        _audioSource.PlayOneShot(_StartSound);
        for (int i = 0; i < GameManager.Instance.KillerPlayers.Count; i++)
        {
            Killers[i].SetActive(true);
            KillersNickNames[i].text = GameManager.Instance.KillerPlayers[i].Name;
        }
        KillerStartTitle.text = GameManager.Instance.KillerPlayers.Count > 1 ?
            "You are <color #9C2116>Killers</color>" :
            "You are <color #9C2116>Killer</color>";
    }
    public void SetCivilianVictory()
    {
        BG.sprite = GreenBg;
        CivilianVictory.SetActive(true);
        CivilianScreen.SetActive(false);
        KillerScreen.SetActive(false);
        _audioSource.PlayOneShot(_CivilianWinSound);
        foreach (var character in Characters)
        {
            character.SetActive(false);
        }
        Characters[GameManager.Instance.LocalPlayer.SkinID].SetActive(true);
        Debug.Log($"Ну что пацанчик у тебя SkinID:{GameManager.Instance.LocalPlayer.SkinID}");
        CivilianNickName.text = GameManager.Instance.LocalPlayer.Name;
    }
    
    public void SetImposterVictory()
    {
        BG.sprite = GrayBg;
        ImposterVictory.SetActive(true);
        CivilianScreen.SetActive(false);
        KillerScreen.SetActive(false);
        CharacterRawImage.SetActive(false);
        _audioSource.PlayOneShot(_ImposterWinSound);
        for (int i = 0; i < GameManager.Instance.KillerPlayers.Count; i++)
        {
            Killers[i].SetActive(true);
            KillersNickNames[i].text = GameManager.Instance.KillerPlayers[i].Name;
        }
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

    public void ActivateScreen()
    {
        WholeScreen.SetActive(true);
    }
    public void DisableScreen()
    {
        WholeScreen.SetActive(false);
    }
    
    private IEnumerator FindAudioSource()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        _audioSource = GameManager.Instance.LocalPlayer.UiaAudioSource;
    }
}
