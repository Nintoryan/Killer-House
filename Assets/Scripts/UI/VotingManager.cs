﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AAPlayer;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Voting
{
    public class VotingManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private PlayerAvatar[] _playerAvatars;
        [SerializeField] private GameObject VotingParent;
        [SerializeField] private GameObject VotingResults;
        [SerializeField] private TMP_Text VotingResultText;
        [SerializeField] private TMP_Text VotingResultRole;
        [SerializeField] private Image Clock;
        [SerializeField] private RectTransform Arrow;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button SendMessageButton;
        [SerializeField] private GameObject SendMessageField;
        
        [SerializeField] private AudioClip VotingStartsSound;
        [SerializeField] private AudioSource _audioSource;
            
        private float timeLeft;
        private PlayerAvatar _localPlayer;
        [SerializeField] private TMP_Text Moto;
        private PlayerAvatar _WhoStartedVoting;
        private bool isSkiped;
        private bool VotingStarted;
        private bool IsVotesShown;
        
        
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
            if (IsVotesShown) return;
            if(_selectedPlayerAvatar == _localPlayer 
               || GameManager.Instance.FindPlayer(_localPlayer.thisPlayerActorID).IsDead
               || _selectedPlayerAvatar == _localPlayer._suspectPlayer) return;
            var sendingData = $"{_localPlayer.thisPlayerActorID}:{_selectedPlayerAvatar.thisPlayerActorID}";
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
            IsVotesShown = false;
            VotingStarted = true;
            VotingParent.SetActive(true);
            _audioSource.PlayOneShot(VotingStartsSound);
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
                if (GameManager.Instance.FindPlayer(PhotonNetwork.LocalPlayer.ActorNumber).IsDead)
                {
                    Moto.text = "You are dead...";
                    skipButton.gameObject.SetActive(false);
                    SendMessageButton.gameObject.SetActive(false);
                    SendMessageField.SetActive(false);
                }
                else
                {
                    Moto.text = "Chose the suspect!";
                }

                Clock.fillAmount = 1;
                Clock.DOFillAmount(0, timeLeft).SetEase(Ease.Linear);
                Arrow.DORotate(new Vector3(0, 0, -360), timeLeft,RotateMode.FastBeyond360).SetRelative().SetEase(Ease.Linear);
                StartCoroutine(WaitUntilVotingEnds());
            }
        }

        private IEnumerator WaitUntilVotingEnds()
        {
            yield return new WaitWhile(() => !isAllVoted());
            OnAllVoted();
        }

        private void OnAllVoted()
        {
            Clock.DOComplete();
            Arrow.DOComplete();
            var s = DOTween.Sequence();
            s.AppendCallback(ShowVotes);
            s.AppendInterval(5f);
            s.AppendCallback(EndVoting);
        }
        
        private void ShowVotes()
        {
            IsVotesShown = true;
            foreach (var playerAvatar in _playerAvatars)
            {
                playerAvatar.WhoVotedParent.SetActive(true);
            }
        }
        private void EndVoting()
        {
            var s = DOTween.Sequence();
            s.AppendCallback(GameManager.Instance._beginEndGame.FadeIn);
            s.AppendInterval(1f);
            s.AppendCallback(() =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    TryKickWorstPlayer();
                }
                VotingParent.SetActive(false);
                VotingResults.SetActive(true);
            });
            s.AppendCallback(GameManager.Instance._beginEndGame.FadeOut);
            s.AppendInterval(7f);
            s.AppendCallback(GameManager.Instance._beginEndGame.FadeIn);
            s.AppendInterval(1f);
            s.AppendCallback(() =>
            {
                VotingResults.SetActive(false);
            });
            s.AppendCallback(GameManager.Instance._beginEndGame.FadeOut);

        }
        
        public void Skip()
        {
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
                    var second = FindPlayerAvatar(Convert.ToInt32(data.Split(':')[1]));
                    SetDependecy(first, second);
                    break;
                case 46:
                    Debug.Log($"Событие начала голосования получено. Данные: {(int)photonEvent.CustomData}");
                    StartVoting();
                    foreach (var player in _playerAvatars)
                    {
                        player.WhoStartedIcon.gameObject.SetActive(false);
                    }
                    _WhoStartedVoting = FindPlayerAvatar((int) photonEvent.CustomData);
                    _WhoStartedVoting.WhoStartedIcon.gameObject.SetActive(true);
                    break;
                case 48:
                    var KickedPlayerActorID = (int) photonEvent.CustomData;
                    Debug.Log($"Событие исключения игрока получено. Данные: {KickedPlayerActorID}");
                    if (KickedPlayerActorID == -1)
                    {
                        VotingResultText.text = "Nobody was ejected";
                        VotingResultRole.gameObject.SetActive(false);
                    }
                    else
                    {
                        VotingResultRole.gameObject.SetActive(true);
                        var KickedPlayer = GameManager.Instance.FindPlayer(KickedPlayerActorID);
                        if (KickedPlayer.isImposter)
                        {
                            Debug.Log($"Исключённый игрок {KickedPlayer} был импостером.");
                            VotingResultRole.text = $"{KickedPlayer.Name} was the <color=red>Killer</color>";
                            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                            var sendOptions = new SendOptions {Reliability = true};
                            var s5 = DOTween.Sequence();
                            s5.AppendInterval(1.5f);
                            s5.AppendCallback(() =>
                            {
                                if(!PhotonNetwork.IsMasterClient) return;
                                PhotonNetwork.RaiseEvent(55,1, options, sendOptions);
                            });
                        }
                        else
                        {
                            Debug.Log($"Исключённый игрок {KickedPlayer} НЕ был импостером.");
                            KickedPlayer.SetDead();
                            KickedPlayer.DisableDeadBody();
                            VotingResultRole.text = $"{KickedPlayer.Name} was the <color=#00BDBBff>Civilian</color>";
                            var AlivePlayers = GameManager.Instance._players.Where(player => !player.isImposter).Count(player => !player.IsDead);
                            if (AlivePlayers <= 1)
                            {
                                var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                                var sendOptions = new SendOptions {Reliability = true};
                                if(!PhotonNetwork.IsMasterClient) return;
                                PhotonNetwork.RaiseEvent(57, 1, options, sendOptions);
                            }
                            else if (PhotonNetwork.IsMasterClient)
                            {
                                //Только если мастер клиент уловил смерть игрока
                                Debug.Log($"Мастер уловил смерть игрока {KickedPlayer}");
                                Debug.Log($"У него {KickedPlayer.AvaliableQuestsAmount} не сделаных заданий, вызываю перераспределение");
                                var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
                                var sendOptions = new SendOptions {Reliability = true};
                                PhotonNetwork.RaiseEvent(65,KickedPlayer.AvaliableQuestsAmount, options, sendOptions);
                            }
                        }
                        VotingResultText.text = $"{KickedPlayer.Name} was ejected";
                    }
                    break;
                case 51:
                    var SkipedPlayerActorID = (int) photonEvent.CustomData;
                    var SkipedPlayer = FindPlayerAvatar(SkipedPlayerActorID);
                    SkipedPlayer.Voted.gameObject.SetActive(false);
                    SkipedPlayer.IsSkiped = true;
                    if (SkipedPlayer._suspectPlayer != null)
                    {
                        SkipedPlayer._suspectPlayer.RemoveFromSuspectedByPlayer(SkipedPlayer.localPlayerNumber);
                        SkipedPlayer._suspectPlayer = null;
                    }
                    if (isAllVoted())
                    {
                        OnAllVoted();
                    }
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
            if (NotOneMaxPlayer)
            {
                RaiseKickedEvent(-1);
            }
            else
            {
                foreach (var p in GameManager.Instance._players.Where(p =>
                    p._photonView.Owner.ActorNumber == _playerAvatars[MaxI].thisPlayerActorID))
                {
                    RaiseKickedEvent(p._photonView.Owner.ActorNumber);
                }
            }
        }
        
        private void SetDependecy(PlayerAvatar fromPlayer, PlayerAvatar toPlayer)
        {
            if (fromPlayer._suspectPlayer != null)
            {
                fromPlayer._suspectPlayer.RemoveFromSuspectedByPlayer(fromPlayer.localPlayerNumber);
            }
            fromPlayer.IsSkiped = false; 
            fromPlayer._suspectPlayer = toPlayer;
            fromPlayer.Voted.gameObject.SetActive(true);
            toPlayer.AddToSuspectedByPlayer(fromPlayer.localPlayerNumber);
        }

        private bool isAllVoted()
        {
            if (timeLeft <= 0)
            {
                VotingStarted = false;
                return true;
            }

            foreach (var t in _playerAvatars)
            {
                if(!t.CanVote) continue;
                if (t._suspectPlayer == null && !t.IsSkiped) return false;
            }
            return true;
        }
        
        private void Update()
        {
            if(VotingStarted)
                timeLeft -= Time.deltaTime;
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
}

