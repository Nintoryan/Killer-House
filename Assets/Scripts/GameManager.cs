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
    public float VotingDuration;
    

    public static GameManager Instance;

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
                foreach (var p in _players.Where(p => p._photonView.Owner.ActorNumber == (int) photonEvent.CustomData))
                {
                    p.SetDead();
                }
                break;
            case 43:
                //Событие начала игры
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
                    if (i == imposterID)
                    {
                        OrderedPlayers[i]._skills.EnableKilling();
                    }
                }
                var s = DOTween.Sequence();
                s.AppendInterval(1f);
                s.AppendCallback(() => LocalPlayer.ActivateControll());
                break;
        }
    }

    public void MovePlayersToSpawn()
    {
        var OrderedPlayers = _players
            .OrderBy(p => p._photonView.Owner.ActorNumber)
            .ToArray();
        LocalPlayer.DisableControll();
        for (int i = 0; i < OrderedPlayers.Length; i++)
        {
            OrderedPlayers[i]._Body.transform.position = new Vector3(
                SpawnPlaces[i].position.x,
                OrderedPlayers[i].transform.position.y,
                SpawnPlaces[i].position.z);
            OrderedPlayers[i].UpdateCameraPos();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        foreach (var p in _players)
        {
            if (p._photonView.Owner.ActorNumber == otherPlayer.ActorNumber)
            {
                _players.Remove(p);
                break;
            }
        }
    }
}
