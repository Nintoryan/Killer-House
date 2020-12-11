using System;
using System.Collections;
using System.Collections.Generic;
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
    [Header("Sounds")]
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _StartSound;
    [SerializeField] private AudioClip _CivilianWinSound;
    [SerializeField] private AudioClip _ImposterWinSound;


    private void Start()
    {
        StartCoroutine(FindAudioSource());
    }

    private IEnumerator FindAudioSource()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        _audioSource = GameManager.Instance.LocalPlayer.UiaAudioSource;
    }

    public void SetCivilianVictory()
    {
        CivilianVictory.SetActive(true);
        CivilianScreen.SetActive(false);
        KillerScreen.SetActive(false);
        _audioSource.PlayOneShot(_CivilianWinSound);
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
        _audioSource.PlayOneShot(_ImposterWinSound);
    }
    public void SetImposterVictory()
    {
        ImposterVictory.SetActive(true);
        CivilianScreen.SetActive(false);
        KillerScreen.SetActive(false);
        _audioSource.PlayOneShot(_ImposterWinSound);
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
        _audioSource.PlayOneShot(_StartSound);
    }

    public void SetKillerScreen(bool isActive)
    {
        KillerScreen.SetActive(isActive);
        Vinet.SetActive(isActive);
        _audioSource.PlayOneShot(_StartSound);
    }

    public void ActivateScreen()
    {
        WholeScreen.SetActive(true);
    }
}
