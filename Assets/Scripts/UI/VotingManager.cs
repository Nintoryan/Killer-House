using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Voting
{
    public class VotingManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private PlayerAvatar[] _playerAvatars;
        [SerializeField] private UILineRenderer _line;
        [SerializeField] private Transform LinesParent;
        [SerializeField] private TMP_Text Timer;
        [SerializeField] private GameObject VotingParent;
        [SerializeField] private GameObject VotingResults;
        [SerializeField] private TMP_Text VotingResultText;
        private float timeLeft;
        private Dictionary<string, UILineRenderer> _playersLines =new Dictionary<string, UILineRenderer>();
        private Image SuspectLine;
        private Image ProtectLine;
        private PointerState _pointerState = PointerState.None;
        private PlayerAvatar _localPlayer;
        private PlayerAvatar _suspectPlayer;
        private PlayerAvatar _protectedPlayer;

        private PlayerAvatar Find(int id)
        {
            var _PlayerAvatar = _playerAvatars.FirstOrDefault(avatar => id == avatar.ID);
            if (_PlayerAvatar == null)
            {
                Debug.Log($"Я не нашёл {id} в массиве {_playerAvatars}");
            }
            return _PlayerAvatar;
        }

        public void VotingEventMeeting()
        {
            string sendingData = "";
            RaiseEventOptions options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(46, sendingData, options, sendOptions);
        }

        public void VotingEventDeadbody(int FoundBodyActorNumber)
        {
            int sendingData = FoundBodyActorNumber;
            RaiseEventOptions options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(47, sendingData, options, sendOptions);
            Debug.Log($"Event 47 was raised! SendingData:{sendingData}");
        }

        private void InitializeVoting(int DeadBodyID = -1)
        {
            Debug.Log("InitializeVoting");
            VotingParent.SetActive(true);
            if (DeadBodyID != -1)
            {
                //Отобразить - кто умер
            }
            timeLeft = GameManager.Instance.VotingDuration;
            var controllers = GameManager.Instance._players
                .OrderBy(p => p._photonView.Owner.ActorNumber)
                .ToArray();
            for (int i = 0; i < _playerAvatars.Length; i++)
            {
                if (i >= controllers.Length)
                {
                    _playerAvatars[i].gameObject.SetActive(false);
                }
                else if (controllers[i].IsDead)
                {
                    _playerAvatars[i].gameObject.SetActive(false);
                }
                else
                {
                    _playerAvatars[i].gameObject.SetActive(true);
                    _playerAvatars[i].Initialize(controllers[i].Name,controllers[i]._skin._Color,controllers[i]._photonView.Owner.ActorNumber);
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

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            Timer.text = timeLeft.ToString("0.0");
        }

        private void EndVoting()
        {
            var s = DOTween.Sequence();
            s.AppendCallback(() =>
            {
                VotingParent.SetActive(false);
                VotingResults.SetActive(true);
                VotingResultText.text = "Nobody was ejected";
            });
            s.AppendInterval(2f);
            s.AppendCallback(() =>
            {
                VotingResults.SetActive(false);
            });

        }

        private void SelectedEvent(PlayerAvatar first, PlayerAvatar second, bool isSus)
        {
            string sendingData = $"{first.ID}:{second.ID}:{(isSus ? 1 : 0)}";
            RaiseEventOptions options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(44, sendingData, options, sendOptions);
            Debug.Log($"Event 44 send {sendingData}");
        }

        private void DrawLine(PlayerAvatar first, PlayerAvatar second, Color _color)
        {
            UILineRenderer newLine;
            if (!_playersLines.ContainsKey($"{first.ID}:{second.ID}"))
            {
                newLine = Instantiate(_line,LinesParent);
                _playersLines.Add($"{first.ID}:{second.ID}",newLine);
            }
            else
            {
                newLine = _playersLines[$"{first.ID}:{second.ID}"];
            }
            newLine.Points = new[]
            {
                first.RectPositionOutcoming,
                second.RectPositionIncoming,
            };
            newLine.color = _color;
        }
        public void SelectPlayerAvatar(PlayerAvatar _selectedPlayerAvatar)
        {
            if(_selectedPlayerAvatar == _localPlayer || _pointerState == PointerState.None) return;
            var isSus = _pointerState == PointerState.Suspect;
            SelectedEvent(_localPlayer,_selectedPlayerAvatar,isSus);
        }
        

        public void SetPointerState(int _state)
        {
            _pointerState = (PointerState)_state;
        }
        
        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 44:
                    var data = (string) photonEvent.CustomData;
                    Debug.Log($"Event 44 get {data}");
                    var first = Find(Convert.ToInt32(data.Split(':')[0]));
                    if (first == null)
                    {
                        Debug.Log($"first is null!!! Parametr:{Convert.ToInt32(data.Split(':')[0])}");
                    }
                    var second = Find(Convert.ToInt32(data.Split(':')[1]));
                    var isSus = data.Split(':')[2] == "1";
                    DrawLine(first, second, isSus ? Color.red : Color.green);
                    if (isSus)
                    {
                        second.AgainstAmount++;
                    }
                    else
                    {
                        second.ForAmount++;
                    }
                    break;
                case 46:
                    InitializeVoting();
                    break;
                case 47:
                    Debug.Log($"Event 47 has gotten! RecivedData:{photonEvent.CustomData}");
                    InitializeVoting();
                    break;
            }
        }
    }

    public enum PointerState
    {
        None = 0,
        Suspect = 1,
        Protect = 2
    }
}

