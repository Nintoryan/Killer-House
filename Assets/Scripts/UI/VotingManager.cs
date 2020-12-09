using System;
using System.Linq;
using AAPlayer;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace Voting
{
    public class VotingManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private PlayerAvatar[] _playerAvatars;
        [SerializeField] private TMP_Text Timer;
        [SerializeField] private GameObject VotingParent;
        [SerializeField] private GameObject VotingResults;
        [SerializeField] private TMP_Text VotingResultText;
        private float timeLeft;
        private DependecieType _dependecieType = DependecieType.None;
        private PlayerAvatar _localPlayer;
        private PlayerAvatar _WhoStartedVoting;
        private bool isSkiped;
        
        
        public static void RaiseVotingEvent(Controller WhoStarted)
        {
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(46, WhoStarted._photonView.Owner.ActorNumber, options, sendOptions);
        }

        private void RaiseSkipEvent()
        {
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(51, _localPlayer.thisPlayerActorID, options, sendOptions);
        }
        public void RaiseSetDependencyEvent(PlayerAvatar _selectedPlayerAvatar)
        {
            if(isSkiped) return;
            if(_selectedPlayerAvatar == _localPlayer || GameManager.Instance.FindPlayer(_localPlayer.thisPlayerActorID).IsDead) return;
            var sendingData = $"{_localPlayer.thisPlayerActorID}:{_selectedPlayerAvatar.thisPlayerActorID}:{(_dependecieType == DependecieType.Suspect ? 1 : 0)}";
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(44, sendingData, options, sendOptions);
        }
        private static void RaiseKickedEvent(int ActorID)
        {
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(48, ActorID, options, sendOptions);
        }

        private void StartVoting()
        {
            VotingParent.SetActive(true);
            timeLeft = GameManager.Instance.VotingDuration;
            var controllers = GameManager.Instance._players
                .OrderBy(p => p._photonView.Owner.ActorNumber)
                .ToArray();
            for (var i = 0; i < _playerAvatars.Length; i++)
            {
                if (i >= controllers.Length)
                {
                    _playerAvatars[i].gameObject.SetActive(false);
                }
                else
                {
                    _playerAvatars[i].gameObject.SetActive(true);
                    _playerAvatars[i].Initialize(controllers[i].Name, controllers[i].LocalNumber, controllers[i]._photonView.Owner.ActorNumber);
                    _playerAvatars[i].localPlayerNumber = i;
                    if (controllers[i]._photonView.IsMine)
                    {
                        _localPlayer = _playerAvatars[i];
                    }
                }
            }
            if (GameManager.Instance.LocalPlayer._photonView.IsMine)
            {
                var s = DOTween.Sequence();
                s.AppendInterval(timeLeft);
                s.AppendCallback(EndVoting); 
            }
            
        }

        private void EndVoting()
        {
            var s = DOTween.Sequence();
            s.AppendCallback(() =>
            {
                TryKickWorstPlayer();
                VotingParent.SetActive(false);
                VotingResults.SetActive(true);
            });
            s.AppendInterval(2f);
            s.AppendCallback(() =>
            {
                VotingResults.SetActive(false);
            });

        }
        public void Skip()
        {
            isSkiped = true;
            RaiseSkipEvent();
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 44:
                    var data = (string) photonEvent.CustomData;
                    Debug.Log($"Событие нажатия на портрет игрока получено. Данные: {data}");
                    var first = FindPlayerAvatar(Convert.ToInt32(data.Split(':')[0]));
                    if (first == null)
                    {
                        Debug.Log($"first is null!!! Parametr:{Convert.ToInt32(data.Split(':')[0])}");
                    }
                    var second = FindPlayerAvatar(Convert.ToInt32(data.Split(':')[1]));
                    var isSus = data.Split(':')[2] == "1";
                    SetDependecy(first, second);
                    break;
                case 46:
                    Debug.Log($"Событие начала голосования получено. Данные: {(int)photonEvent.CustomData}");
                    StartVoting();
                    _WhoStartedVoting = FindPlayerAvatar((int) photonEvent.CustomData);
                    _WhoStartedVoting.WhoStartedIcon.gameObject.SetActive(true);
                    break;
                case 48:
                    var KickedPlayerActorID = (int) photonEvent.CustomData;
                    Debug.Log($"Событие исключения игрока получено. Данные: {KickedPlayerActorID}");
                    var KickedPlayer = GameManager.Instance.FindPlayer(KickedPlayerActorID);
                    KickedPlayer.SetDead();
                    KickedPlayer.DisableDeadBody();
                    VotingResultText.text = $"{KickedPlayer.Name} was ejected";
                    break;
                case 51:
                    var SkipedPlayerActorID = (int) photonEvent.CustomData;
                    FindPlayerAvatar(SkipedPlayerActorID).Skiped.gameObject.SetActive(true);
                    break;
            }
        }

        private void TryKickWorstPlayer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            var MaxKickScore = -100;
            var MaxI = 0;
            var NotOneMaxPlayer = false;
            for (var i = 0; i < _playerAvatars.Length; i++)
            {
                if (_playerAvatars[i].KickScore <= MaxKickScore) continue;
                MaxKickScore = _playerAvatars[i].KickScore;
                MaxI = i;
            }
            for (var i = 0; i < _playerAvatars.Length; i++)
            {
                if(i==MaxI) continue;
                if (_playerAvatars[i].KickScore == MaxKickScore)
                {
                    NotOneMaxPlayer = true;
                }
            }
            if (NotOneMaxPlayer) return;
            foreach (var p in GameManager.Instance._players.Where(p =>
                p._photonView.Owner.ActorNumber == _playerAvatars[MaxI].thisPlayerActorID))
            {
                RaiseKickedEvent(p._photonView.Owner.ActorNumber);
            }
        }
        
        private static void SetDependecy(PlayerAvatar fromPlayer, PlayerAvatar toPlayer)
        {
            if (fromPlayer._suspectPlayer != null)
            {
                fromPlayer._suspectPlayer.suspectedByPlayersID.Remove(fromPlayer.localPlayerNumber);
                fromPlayer._suspectPlayer.KickScore--;
            }
            fromPlayer._suspectPlayer = toPlayer;
            fromPlayer.Voted.gameObject.SetActive(true);
            toPlayer.suspectedByPlayersID.Add(fromPlayer.localPlayerNumber);
            toPlayer.KickScore++;
        }
        public void SetDependencyType(int _state)
        {
            _dependecieType = (DependecieType)_state;
        }
        private void Update()
        {
            timeLeft -= Time.deltaTime;
            Timer.text = timeLeft.ToString("0.0");
        }
        private PlayerAvatar FindPlayerAvatar(int ActorID)
        {
            var _PlayerAvatar = _playerAvatars.FirstOrDefault(avatar => ActorID == avatar.thisPlayerActorID);
            if (_PlayerAvatar == null)
            {
                Debug.Log($"Я не нашёл {ActorID} в массиве {_playerAvatars}");
            }
            return _PlayerAvatar;
        }
    }

    public enum DependecieType
    {
        None = 0,
        Suspect = 1,
        Protect = 2
    }
}

