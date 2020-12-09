using System.Collections.Generic;
using System.Linq;
using AAPlayer;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using DG.Tweening;
public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public List<Controller> _players = new List<Controller>();
    public Controller LocalPlayer;
    public Transform[] SpawnPlaces;
    public GameObject AmountOfPlayers;
    public int QuestsAmountForEachPlayer = 5;
    public float VotingDuration;
    public MinigameZone[] AllMinigames;
    public List<MinigameZone> MyMinigames;
    public int AmountOfDoneQuests;
    public int AmountOfQuests;
    public BeginEndGame _beginEndGame;

    public static GameManager Instance;
    private bool isGameStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void AddPlayer(Controller Player)
    {
        _players.Add(Player);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 42:
                //Событие смерти игрока
                var KilledPlayer = FindPlayer((int) photonEvent.CustomData);
                KilledPlayer.SetDead();
                if (KilledPlayer.isImposter)
                {
                    var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                    var sendOptions = new SendOptions {Reliability = true};
                    PhotonNetwork.RaiseEvent(55,1, options, sendOptions);
                }
                else
                {
                    var AlivePlayers = _players.Where(player => !player.isImposter).
                        Count(player => !player.IsDead);
                    if (AlivePlayers == 1)
                    {
                        var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                        var sendOptions = new SendOptions {Reliability = true};
                        PhotonNetwork.RaiseEvent(57,1, options, sendOptions);
                    }
                }
                break;
            case 43:
                //Событие начала игры
                var s = DOTween.Sequence();
                s.AppendCallback(_beginEndGame.FadeIn);
                s.AppendInterval(1.5f);
                s.AppendCallback(() =>
                {
                    _beginEndGame.ActivateScreen();
                    AmountOfPlayers.SetActive(false);
                    var OrderedPlayers = _players
                        .OrderBy(p => p._photonView.Owner.ActorNumber)
                        .ToArray();
                    LocalPlayer.DisableControll();
                    int imposterID = (int) photonEvent.CustomData;
                    for (int i = 0; i < OrderedPlayers.Length; i++)
                    {
                        OrderedPlayers[i]._Body.transform.position = new Vector3(
                            SpawnPlaces[i].position.x,
                            OrderedPlayers[i].transform.position.y,
                            SpawnPlaces[i].position.z);
                        OrderedPlayers[i].UpdateCameraPos();
                        OrderedPlayers[i]._skills.SetKillingActive(i == imposterID);
                        OrderedPlayers[i]._skills.SetAlarmButtonActive(true);
                        OrderedPlayers[i]._skills.SetInteractButtonActive(i != imposterID);
                        OrderedPlayers[i]._skills.SetDomofonButtonActive(i == imposterID);
                        OrderedPlayers[i].isImposter = i == imposterID;
                        _beginEndGame.SetCharacterImageActive(OrderedPlayers[i].LocalNumber,OrderedPlayers[i].Name);
                    }
                    
                    if (LocalPlayer.isImposter)
                    {
                        _beginEndGame.SetKillerScreen(true);
                    }
                    else
                    {
                        _beginEndGame.SetCivilianScreen(true);
                        while(MyMinigames.Count < QuestsAmountForEachPlayer){
                            var minigame = AllMinigames[Random.Range(0, AllMinigames.Length)];
                            if (!MyMinigames.Contains(minigame))
                            {
                                MyMinigames.Add(minigame);
                                LocalPlayer._InGameUI.SetMarkActive(minigame.Number);
                            }
                        }
                    }
                    AmountOfQuests = (OrderedPlayers.Length-1) * QuestsAmountForEachPlayer;
                });
                s.AppendCallback(_beginEndGame.FadeOut);
                s.AppendInterval(3f);
                s.AppendCallback(_beginEndGame.FadeIn);
                s.AppendInterval(1.5f);
                s.AppendCallback(_beginEndGame.DisableScreen);
                s.AppendCallback(_beginEndGame.FadeOut);
                s.AppendInterval(1);
                s.AppendCallback(() =>
                {
                    LocalPlayer.ActivateControll();
                    if (LocalPlayer.isImposter)
                    {
                        LocalPlayer._skills.KillGoCD();
                    }
                    isGameStarted = true;
                });
                break;
            case 50:
                //Событие выключения мертвого тела
                var DeadPlayer = FindPlayer((int) photonEvent.CustomData);
                DeadPlayer.DisableDeadBody();
                break;
            case 53:
                //Событие успешного завершения квеста
                AmountOfDoneQuests++;
                LocalPlayer._InGameUI.SetProgress((float)AmountOfDoneQuests/AmountOfQuests);
                if (AmountOfDoneQuests == AmountOfQuests)
                {
                    var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                    var sendOptions = new SendOptions {Reliability = true};
                    PhotonNetwork.RaiseEvent(55,1, options, sendOptions);
                }
                break;
            case 55:
                //Событие победы мирных
                LocalPlayer.DisableControll();
               var s1 = DOTween.Sequence();
                s1.AppendCallback(_beginEndGame.FadeIn);
                s1.AppendInterval(1.5f);
                s1.AppendCallback(() =>
                {
                    _beginEndGame.ActivateScreen();
                    _beginEndGame.SetGreenBG();
                    _beginEndGame.SetCivilianVictory();
                    var OrderedPlayers = _players
                        .OrderBy(p => p._photonView.Owner.ActorNumber).ToArray();
                    LocalPlayer.DisableControll();
                    foreach (var t in OrderedPlayers)
                    {
                        _beginEndGame.SetCharacterImageActive(t.LocalNumber,t.Name);
                        if (t.IsDead)
                        {
                            _beginEndGame.SetCharacteImageDead(t.LocalNumber);
                        }
                    }
                });
                s1.AppendCallback(_beginEndGame.FadeOut);
                s1.AppendInterval(5f);
                s1.AppendCallback(_beginEndGame.FadeIn);
                s1.AppendInterval(3);
                s1.AppendCallback(() => LocalPlayer._InGameUI.Leave());
                break;
            case 57:
                //Событие победы импостера
                LocalPlayer.DisableControll();
                var s2 = DOTween.Sequence();
                s2.AppendCallback(_beginEndGame.FadeIn);
                s2.AppendInterval(1.5f);
                s2.AppendCallback(() =>
                {
                    _beginEndGame.ActivateScreen();
                    _beginEndGame.SetGrayBG();
                    _beginEndGame.TurnAllPortraitsOff();
                    var OrderedPlayers = _players
                        .OrderBy(p => p._photonView.Owner.ActorNumber).ToArray();
                    foreach (var t in OrderedPlayers)
                    {
                        if (t.isImposter)
                        {
                            if (LocalPlayer.isImposter)
                            {
                                _beginEndGame.SetImposterVictory();
                            }
                            else
                            {
                                _beginEndGame.SetCivilianDefeat();
                            }
                            _beginEndGame.SetCharacterImageActive(t.LocalNumber,t.Name);
                            _beginEndGame.SetCharacterImageRed(t.LocalNumber);
                        }
                    }
                });
                s2.AppendCallback(_beginEndGame.FadeOut);
                s2.AppendInterval(5f);
                s2.AppendCallback(_beginEndGame.FadeIn);
                s2.AppendInterval(3);
                s2.AppendCallback(() => LocalPlayer._InGameUI.Leave());
                break;

        }
    }

    public Controller FindPlayer(int ActorID)
    {
        var Player = _players.FirstOrDefault(avatar => avatar._photonView.Owner.ActorNumber == ActorID);
        if (Player == null)
        {
            Debug.Log($"Я не нашёл {ActorID} в массиве {_players}");
        }
        return Player;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        var p = FindPlayer(otherPlayer.ActorNumber);
        if (isGameStarted)
        {
            if (p.isImposter)
            {
                var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                var sendOptions = new SendOptions {Reliability = true};
                PhotonNetwork.RaiseEvent(55,1, options, sendOptions);
            }
            else
            {
                var AlivePlayers = _players.Where(player => !player.isImposter).Count(player => !player.IsDead);
                if (AlivePlayers == 1)
                {
                    var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                    var sendOptions = new SendOptions {Reliability = true};
                    PhotonNetwork.RaiseEvent(57, 1, options, sendOptions);
                }
            }
        }
        _players.Remove(p);
        Destroy(p.gameObject);
    }
}
