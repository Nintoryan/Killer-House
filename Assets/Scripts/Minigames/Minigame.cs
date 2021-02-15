using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class Minigame : MonoBehaviour
{
    public int Number;
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject Guide;
    [SerializeField] private GameObject Interface;
    [SerializeField] protected MinigamesManager _minigamesManager;
    protected bool isMiniGameStarted;

    public virtual void InitializeMiniGame()
    {
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
        gm.LocalPlayer._skills.SetInteractButtonInteractable(false);
        gm.LocalPlayer._InGameUI.SetMarkDisables(Number);
        gm.AllMinigames[Number].QuestSign.SetActive(false);
        var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        var sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(53,gm.LocalPlayer._photonView.Owner.ActorNumber , options, sendOptions);
    }
    public void Win()
    {
        WinScreen.SetActive(true);
        WinScreen.GetComponentInChildren<TMP_Text>().text = "Done!";
        _minigamesManager.PlayCompleteSound();
        isMiniGameStarted = false;
        var s = DOTween.Sequence();
        s.AppendInterval(2f);
        s.AppendCallback(() =>
        {
            _minigamesManager.Close();
        });
        Complete();

        Interface.SetActive(false);
    }
}
