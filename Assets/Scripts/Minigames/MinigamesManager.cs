using UnityEngine;

public class MinigamesManager : MonoBehaviour
{
    public Minigame[] AllMinigames;
    public GameObject MiniGamesCanvas;
    public int CurrentMinigameID = -1;
    [SerializeField] private AudioClip[] TapSounds;
    [SerializeField] private AudioClip OnCompleteSound;
    [SerializeField] private AudioSource _audioSource; 
    public void OpenMinigame()
    {
        if (CurrentMinigameID == -1)
        {
            Debug.LogWarning("Всё плохо, запустилась минигра без номера!");
            return;
        }
        MiniGamesCanvas.SetActive(true);
        AllMinigames[CurrentMinigameID].gameObject.SetActive(true);
        AllMinigames[CurrentMinigameID].InitializeMiniGame();
    }

    public void Close()
    {
        MiniGamesCanvas.SetActive(false);
        GameManager.Instance.AllMinigames[CurrentMinigameID]._MinigamePresenter.Stop();
        GameManager.Instance.AllMinigames[CurrentMinigameID]._MinigamePresenter.gameObject.SetActive(false);
        AllMinigames[CurrentMinigameID].gameObject.SetActive(false);
    }

    public void PlaySound(int id)
    {
        _audioSource.PlayOneShot(TapSounds[id]);
    }

    public void PlayCompleteSound()
    {
        _audioSource.PlayOneShot(OnCompleteSound);
    }
}
