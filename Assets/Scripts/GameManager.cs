using System;
using System.Collections.Generic;
using System.Linq;
using AAPlayer;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public List<Controller> _players = new List<Controller>();
    public DomofonZone[] _DomofonZones;
    public Controller LocalPlayer;
    [FormerlySerializedAs("KillerPlayer")] public List<Controller> KillerPlayers = new List<Controller>();
    public Transform[] SpawnPlaces;
    public GameObject AmountOfPlayers;
    public GameObject AmountOfKillersText;
    public int QuestsAmountForEachPlayer = 5;
    public float VotingDuration;
    public MinigameZone[] AllMinigames;
    public List<MinigameZone> MyMinigames;
    public ShortCutZone[] AllShortCutZones;
    public LobbyManager _LobbyManager;
    public GameObject VotingSign;

    public float TimeSinceGameStarted;
    public List<int> MyMinigamesIDs
    {
        get
        {
            return MyMinigames.Select(mg => mg.Number).ToList();
        }
    }
    public int AmountOfDoneQuests;
    public int AmountOfQuests;
    public int AmountOfKillers;
    public int Progress => Mathf.RoundToInt((float) AmountOfDoneQuests / AmountOfQuests * 100f);
    public BeginEndGame _beginEndGame;

    public static GameManager Instance;
    public bool isGameStarted;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            var props = new Hashtable {{"HostAmountOfKillers", PlayerPrefs.GetInt("HostAmountOfKillers")}};
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            AmountOfKillers = PlayerPrefs.GetInt("HostAmountOfKillers");
        }
        else
        {
            AmountOfKillers = (int)PhotonNetwork.CurrentRoom.CustomProperties["HostAmountOfKillers"];
        }
        AmountOfKillersText.GetComponent<TMP_Text>().text = "Killers:" + AmountOfKillers;
    }

    private void Update()
    {
        TimeSinceGameStarted += Time.deltaTime;
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
                OnPlayerDead((int) photonEvent.CustomData);
                break;
            case 43:
                var data = (object[]) photonEvent.CustomData;
                var KillersIDs = new List<int>{(int)data[0],(int)data[1],(int)data[2]};
                OnStartGame(KillersIDs);
                break;
            case 50:
                //Событие выключения мертвого тела
                var DeadPlayer = FindPlayer((int) photonEvent.CustomData);
                DeadPlayer.DisableDeadBody();
                break;
            case 53:
                //Событие успешного завершения квеста
                FindPlayer((int) photonEvent.CustomData).AvaliableQuestsAmount--;
                AmountOfDoneQuests++;
                LocalPlayer._InGameUI.SetProgress((float)AmountOfDoneQuests/AmountOfQuests);
                if (AmountOfDoneQuests == AmountOfQuests)
                {
                    var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                    var sendOptions = new SendOptions {Reliability = true};
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.RaiseEvent(55,1, options, sendOptions);
                    }
                }
                break;
            case 55:
                OnCiviliansWin();
                break;
            case 57:
                OnImposterWin();
                break;
            case 65:
                //Событие распределения квестов
                if(!PhotonNetwork.IsMasterClient) return;
                var QuestsToDistibute = (int) photonEvent.CustomData;
                Debug.Log($"Мастер получил запрос на перераспределение {QuestsToDistibute} заданий");
                for (int i = 0; i < QuestsToDistibute; i++)
                {    
                    var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                    var sendOptions = new SendOptions {Reliability = true};
                    PhotonNetwork.RaiseEvent(70,AliveCivilians[i % AliveCivilians.Length]._photonView.Owner.ActorNumber, options, sendOptions);
                }
                break;
            case 67:
                //Событие использования домофона
                _DomofonZones[(int)photonEvent.CustomData].GetEventUse();
                break;
            case 70:
                //Событие получения нового задания
                var ActorIDNumber = (int)photonEvent.CustomData;
                var QuestGeter = FindPlayer(ActorIDNumber);
                if (QuestGeter == LocalPlayer)
                {
                    var RandomID = Random.Range(0,AllMinigames.Length);
                    int interationsAmount = 0;
                    while (MyMinigamesIDs.Contains(RandomID))
                    {
                        RandomID = Random.Range(0,AllMinigames.Length);
                        interationsAmount++;
                        if (interationsAmount >= 10)
                        {
                            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                            var sendOptions = new SendOptions {Reliability = true};
                            PhotonNetwork.RaiseEvent(53,LocalPlayer._photonView.Owner.ActorNumber, options, sendOptions);
                            return;
                        }
                    }
                    MyMinigames.Add(AllMinigames[RandomID]);
                    LocalPlayer._InGameUI.SetMarkActive(RandomID);
                    AllMinigames[RandomID].QuestSign.SetActive(true);
                    Debug.Log($"Игрок получил событие добавления нового задания ID:{RandomID}");
                }
                else
                {
                    QuestGeter.AvaliableQuestsAmount++;
                }
                break;
            case 75:
                var data1 = (object[])photonEvent.CustomData;
                AllShortCutZones[(int)data1[0]].GetEventUse((int)data1[1]);
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        LocalPlayer._InGameUI.ShowPlayerJoinLeave($"{newPlayer.NickName} joined the game");
    }

    public void MovePlayersOnSpawn()
    {
        var OrderedPlayers = _players
            .OrderBy(p => p._photonView.Owner.ActorNumber)
            .ToArray();
        for (int i = 0; i < OrderedPlayers.Length; i++)
        {
            OrderedPlayers[i]._Body.transform.position = new Vector3(
                SpawnPlaces[i].position.x,
                OrderedPlayers[i].transform.position.y,
                SpawnPlaces[i].position.z);
            OrderedPlayers[i].UpdateCameraPos();
        }
    }

    private void CheckGameEnded()
    {
        //Проверка на конец игры
        if(!PhotonNetwork.IsMasterClient) return;
        var AlivePlayers = _players.Where(player => !player.isImposter).
            Count(player => !player.IsDead);
        if (AlivePlayers <= 1)
        {
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(57,1, options, sendOptions);
            }
        }

        var AliveImposters = _players.Where(player => player.isImposter).Count(player => !player.IsDead);
        if (AliveImposters == 0)
        {
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(55,1, options, sendOptions);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        LocalPlayer._InGameUI.ShowPlayerJoinLeave($"{otherPlayer.NickName} left the game");
        var p = FindPlayer(otherPlayer.ActorNumber);
        if (isGameStarted)
        {
            CheckGameEnded();
            if (p == null)
            {
                Debug.Log("Ой а всё, а он уже ливнул");
                return;
            }
            if (PhotonNetwork.IsMasterClient && !p.IsDead)
            {
                //Только если мастер клиент уловил уничтожение игрока
                var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                var sendOptions = new SendOptions {Reliability = true};
                PhotonNetwork.RaiseEvent(65, p.AvaliableQuestsAmount, options, sendOptions);
            }
        }
        try
        {
            _players.Remove(p);
            Destroy(p.gameObject);
        }
        catch (Exception e)
        {
            Console.WriteLine("Объект уже уничтожен!");
        }
    }

    private void OnPlayerDead(int DeadPlayerID)
    {
        //Событие смерти игрока
        var KilledPlayer = FindPlayer(DeadPlayerID);
        if (KilledPlayer == LocalPlayer)
        {
            LocalPlayer.SetCamFOV(24);
        }
        KilledPlayer.SetDead();
        if (PhotonNetwork.IsMasterClient)
        {
            CheckGameEnded();
            //Только если мастер клиент уловил смерть игрока
            Debug.Log($"Мастер уловил смерть игрока {KilledPlayer}");
            Debug.Log(
                $"У него {KilledPlayer.AvaliableQuestsAmount} не сделаных заданий, вызываю перераспределение");
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(65, KilledPlayer.AvaliableQuestsAmount, options, sendOptions);
        }
    }

    private void OnStartGame(List<int> KillersIDs)
    {
        //Событие начала игры
                LocalPlayer.DisableControll();
                var s = DOTween.Sequence();
                s.AppendCallback(_beginEndGame.FadeIn);
                s.AppendInterval(1.5f);
                s.AppendCallback(() =>
                {
                    TimeSinceGameStarted = 0;
                    var metrica = AppMetrica.Instance;
                    PlayerPrefs.SetInt("levelNumber",PlayerPrefs.GetInt("levelNumber")+1);
                    var paramerts = new Dictionary<string, object>
                    {
                        {"level", PlayerPrefs.GetInt("levelNumber")}
                    };
                    metrica.ReportEvent("level_start",paramerts);
                    metrica.SendEventsBuffer();
                    
                    isGameStarted = true;
                    VotingSign.SetActive(true);
                    _beginEndGame.ActivateScreen();
                    AmountOfPlayers.SetActive(false);
                    AmountOfKillersText.SetActive(false);
                    var OrderedPlayers = _players
                        .OrderBy(p => p._photonView.Owner.ActorNumber)
                        .ToArray();
                    var imposterIDs = KillersIDs;
                    int impostersAdded = 0;
                    for (int i = 0; i < OrderedPlayers.Length; i++)
                    {
                        OrderedPlayers[i]._Body.transform.position = new Vector3(
                            SpawnPlaces[i].position.x,
                            OrderedPlayers[i].transform.position.y,
                            SpawnPlaces[i].position.z);
                        OrderedPlayers[i].UpdateCameraPos();
                        if (imposterIDs.Contains(i) && impostersAdded < AmountOfKillers)
                        {
                            //Киллер
                            KillerPlayers.Add(OrderedPlayers[i]);
                            OrderedPlayers[i].isImposter = true;
                            OrderedPlayers[i]._skills.SetDomofonButtonActive(true);
                            OrderedPlayers[i]._skills.SetShortCutButtonActive(true);
                            OrderedPlayers[i]._skills.SetKillingActive(true);
                            OrderedPlayers[i]._skills.SetInteractButtonActive(false);
                            impostersAdded++;
                        }
                        else
                        {
                            //Мирный
                            OrderedPlayers[i].isImposter = false;
                            OrderedPlayers[i].AvaliableQuestsAmount = QuestsAmountForEachPlayer;
                            OrderedPlayers[i]._skills.SetDomofonButtonActive(false);
                            OrderedPlayers[i]._skills.SetShortCutButtonActive(false);
                            OrderedPlayers[i]._skills.SetKillingActive(false);
                            OrderedPlayers[i]._skills.SetInteractButtonActive(true);
                        }
                        OrderedPlayers[i]._skills.SetAlarmButtonActive(true);
                    }
                    if (LocalPlayer.isImposter)
                    {
                        _beginEndGame.StartKillerScreen();
                        foreach (var mg in AllMinigames)
                        {
                            mg.QuestSign.SetActive(true);
                        }
                        LocalPlayer.SetCamFOV(24);
                    }
                    else
                    {
                        _beginEndGame.StartCivilianScreen();
                        while(MyMinigames.Count < QuestsAmountForEachPlayer){
                            var minigame = AllMinigames[Random.Range(0, AllMinigames.Length)];
                            if (!MyMinigames.Contains(minigame))
                            {
                                MyMinigames.Add(minigame);
                                LocalPlayer._InGameUI.SetMarkActive(minigame.Number);
                                minigame.QuestSign.SetActive(true);
                            }
                        }
                    }
                    AmountOfQuests = (OrderedPlayers.Length-1) * QuestsAmountForEachPlayer;
                    LocalPlayer._chat.ChatParent.SetActive(false);
                    LocalPlayer._skills.ChatButton.gameObject.SetActive(false);
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
                });
    }

    private void OnImposterWin()
    {
        //Событие победы импостера
        LocalPlayer.DisableControll();
        var s2 = DOTween.Sequence();
        s2.AppendCallback(_beginEndGame.FadeIn);
        s2.AppendInterval(1.5f);
        s2.AppendCallback(() =>
        {
            _beginEndGame.SetImposterVictory();
            LocalPlayer.DisableControll();
        });
        s2.AppendCallback(_beginEndGame.FadeOut);
        s2.AppendInterval(5f);
        s2.AppendCallback(_beginEndGame.FadeIn);
        s2.AppendInterval(3);
        s2.AppendCallback(() => { LocalPlayer._InGameUI.Leave(LocalPlayer.isImposter ? "win" : "lose"); });
    }

    private void OnCiviliansWin()
    {
        //Событие победы мирных
        LocalPlayer.DisableControll();
        var s1 = DOTween.Sequence();
        s1.AppendCallback(_beginEndGame.FadeIn);
        s1.AppendInterval(1.5f);
        s1.AppendCallback(() =>
        {
            _beginEndGame.SetCivilianVictory();
            LocalPlayer.DisableControll();
        });
        s1.AppendCallback(_beginEndGame.FadeOut);
        s1.AppendInterval(5f);
        s1.AppendCallback(_beginEndGame.FadeIn);
        s1.AppendInterval(3);
        s1.AppendCallback(() => LocalPlayer._InGameUI.Leave(LocalPlayer.isImposter ? "lose" : "win"));
    }
    private Controller[] AliveCivilians =>_players.Where(p=>!p.IsDead && !p.isImposter)
        .OrderBy(p => p._photonView.Owner.ActorNumber)
        .ToArray();
}
