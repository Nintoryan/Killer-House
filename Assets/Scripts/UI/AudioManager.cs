using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private bool isMusicOn = true;
    [SerializeField] private Sprite EnabledMusic;
    [SerializeField] private Sprite DisabledMusic;
    [SerializeField] private Button MusicButton;
    public AudioMixer _AudioMixer;


    private void Start()
    {
        if (PlayerPrefs.GetInt("SoundsVolume") == 0)
        {
            MusicButton.GetComponent<Image>().sprite = EnabledMusic;
            _AudioMixer.SetFloat("volume", 0);
            isMusicOn = true;
        }
        else
        {
            MusicButton.GetComponent<Image>().sprite = DisabledMusic;
            _AudioMixer.SetFloat("volume", -80);
            isMusicOn = false;
        }
    }

    public void TurnMusic()
    {
        isMusicOn = !isMusicOn;
        MusicButton.GetComponent<Image>().sprite = isMusicOn ? EnabledMusic : DisabledMusic;
        if (isMusicOn)
        {
            //Включить музыку
            _AudioMixer.SetFloat("volume", 0);
            PlayerPrefs.SetInt("SoundsVolume",0);
        }
        else
        {
            //Выключить музыку
            _AudioMixer.SetFloat("volume", -80);
            PlayerPrefs.SetInt("SoundsVolume",-80);
        }
    }
}
