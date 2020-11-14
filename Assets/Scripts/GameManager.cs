using System;
using System.Collections.Generic;
using AAPlayer;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public List<Controller> _players = new List<Controller>();

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
                Debug.Log($"Надо убить игрока {(int) photonEvent.CustomData}");
                foreach (var p in _players)
                {
                    Debug.Log($"Проверяю игрока {p._photonView.Owner.ActorNumber}");
                    if (p._photonView.Owner.ActorNumber == (int) photonEvent.CustomData)
                    {
                        p.SetDead();
                    }
                }
                break;
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
