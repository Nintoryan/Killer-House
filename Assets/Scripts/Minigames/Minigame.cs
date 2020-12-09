using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
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
        isMiniGameStarted = false;
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.gameObject.SetActive(true);
    }

    public virtual void StartMinigame()
    {
        isMiniGameStarted = true;
        Guide.SetActive(false);
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.Play();
    }
    private void Complete()
    {
        var gm = GameManager.Instance;
        gm.AllMinigames[Number].isComplete = true;
        gm.LocalPlayer._skills._interactButton.interactable = false;
        gm.LocalPlayer._InGameUI.SetMarkDisables(Number);
        var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        var sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(53,gm.LocalPlayer._photonView.Owner.ActorNumber , options, sendOptions);
    }
    public void Win()
    {
        WinScreen.SetActive(true);
        isMiniGameStarted = false;
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.Stop();
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.gameObject.SetActive(false);
        Complete();
    }

    public void Fail()
    {
        FailScreen.SetActive(true);
        isMiniGameStarted = false;
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.Stop();
        GameManager.Instance.AllMinigames[Number]._MinigamePresenter.gameObject.SetActive(false);
    }
}
