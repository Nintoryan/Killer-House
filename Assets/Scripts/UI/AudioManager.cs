using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private bool isMusicOn = true;
    [SerializeField] private Sprite EnabledMusic;
    [SerializeField] private Sprite DisabledMusic;
    [SerializeField] private Button MusicButton;
    

    public void TurnMusic()
    {
        isMusicOn = !isMusicOn;
        MusicButton.GetComponent<Image>().sprite = isMusicOn ? EnabledMusic : DisabledMusic;
        if (isMusicOn)
        {
            //Включить музыку
        }
        else
        {
            //Выключить музыку
        }
    }
}
