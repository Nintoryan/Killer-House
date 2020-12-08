using UnityEngine;
using UnityEngine.UI;

public class MinigamesManager : MonoBehaviour
{
    public Minigame[] AllMinigames;
    public RenderTexture[] AllAnimations;
    public GameObject MiniGamesCanvas;
    public RawImage Animation;
    public int CurrentMinigameID = -1;
    public void OpenMinigame()
    {
        if (CurrentMinigameID == -1)
        {
            Debug.LogWarning("Всё плохо, запустилась минигра без номера!");
            return;
        }
        MiniGamesCanvas.SetActive(true);
        Animation.texture = AllAnimations[CurrentMinigameID];
        AllMinigames[CurrentMinigameID].InitializeMiniGame();
    }
}
