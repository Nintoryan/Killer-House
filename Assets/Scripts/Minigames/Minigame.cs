using UnityEngine;

public class Minigame : MonoBehaviour
{
    public int Number;
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject Guide;
    [SerializeField] private GameObject FailScreen;
    [SerializeField] private GameObject Interface;
    protected bool isMiniGameStarted;

    public virtual void InitializeMiniGame()
    {
        FailScreen.SetActive(false);
        WinScreen.SetActive(false);
        Guide.SetActive(true);
        Interface.SetActive(true);
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.gameObject.SetActive(true);
    }

    protected virtual void StartMinigame()
    {
        isMiniGameStarted = true;
        Guide.SetActive(false);
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.Play();
    }
    private void Complete()
    {
        GameManager.Instance.AllMinigames[Number].isComplete = true;
        GameManager.Instance.LocalPlayer._skills._interactButton.interactable = false;
    }
    protected void Win()
    {
        WinScreen.SetActive(true);
        isMiniGameStarted = false;
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.Stop();
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.gameObject.SetActive(false);
        Complete();
    }

    protected void Fail()
    {
        FailScreen.SetActive(true);
        isMiniGameStarted = false;
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.Stop();
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.gameObject.SetActive(false);
    }
}
