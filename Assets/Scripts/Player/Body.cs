﻿using System.Collections.Generic;
//using Photon.Pun;
using UnityEngine;
#pragma warning disable CS0649
namespace AAPlayer
{
    public class Body : MonoBehaviour//,IPunObservable
    {
        public Controller _Controller;
        [SerializeField] private Skills _skills;
        [SerializeField] private GameObject _Graphics;
        [SerializeField] private GameObject[] AllSkins;
        [SerializeField] private MinigamesManager _minigamesManager;
        private List<Controller> _deadBodies = new List<Controller>();
        public bool HiddenDeadBody;
        //public bool IsInShortCut;

        private DomofonZone CurrentDomofon;

        public Animator _Animator => _Graphics.GetComponent<Animator>();
        
        public void Initialize(int id)
        {
            foreach (var t in AllSkins)
            {
                t.SetActive(false);
            }
            AllSkins[id].SetActive(true);
            _Graphics = AllSkins[id];
        }
        public void Hide()
        {
            _Graphics.SetActive(false);
            _Controller.DisableNickName();
        }

        public void Show()
        {
            if(HiddenDeadBody) return;
            _Graphics.SetActive(true);
            _Controller.ActivateNickName();
        }

        /*private void FixedUpdate()
        {
            if (!_Controller._photonView.IsMine) return;

            if (_Controller.IsDead)
            {
                foreach (var player in GameManager.Instance._players)
                {
                    player._Body.Show();
                }
                return;
            }
            foreach (var player in GameManager.Instance._players)
            {
                if(player == _Controller) continue;
                if (player._Body.IsInShortCut)
                {
                    player._Body.gameObject.SetActive(false);
                    player.NickNameCanvas.gameObject.SetActive(false);
                }
                else
                {
                    player._Body.gameObject.SetActive(true);
                    player.NickNameCanvas.gameObject.SetActive(true);
                }
                int layerMask = 1 << 8;
                if (Physics.Linecast(transform.position, player._Body.transform.position, layerMask))
                {
                    player._Body.Hide();
                }
                else
                {
                    player._Body.Show();
                }
            }
        }*/
        private void OnTriggerEnter(Collider other)
        {
            var body = other.GetComponent<DeadBodyZone>();
            if (body != null)
            {
                if (!_deadBodies.Contains(body._Controller))
                {
                    _deadBodies.Add(body._Controller);
                }
                _skills.SetAlarmButtonInteractable(true);
            }

            if (other.GetComponent<VotingZone>() != null && !_skills.HadSpawnAlarm)
            {
                _skills.SetAlarmButtonInteractable(true);
            }
            var MiniGame = other.GetComponent<MinigameZone>();
            if (MiniGame != null)
            {
                if (!MiniGame.isComplete && GameManager.Instance.MyMinigames.Contains(MiniGame))
                {
                    _skills.SetInteractButtonInteractable(true);
                    _minigamesManager.CurrentMinigameID = MiniGame.Number;
                }
            }

            var Domofon = other.GetComponent<DomofonZone>();
            if (Domofon != null && _Controller.isImposter)
            {
                CurrentDomofon = Domofon;
                _skills.SetDomofonButtonInteracteble(true);
            }

            var sczone = other.GetComponent<ShortCutZone>();
            if (sczone != null && _Controller.isImposter)
            {
                _skills.SetShortCutButtonInteractable(true);
                myShortCutZone = sczone;
            }
        }
        
    
        public Controller GetDeadBody()
        {
            return _deadBodies.Count > 0 ? _deadBodies[0] : null;
        }

        private void OnTriggerExit(Collider other)
        {
            var body = other.GetComponent<DeadBodyZone>();
            if (body != null)
            {
                if (_deadBodies.Contains(body._Controller))
                {
                    _deadBodies.Remove(body._Controller);
                }
                _skills.SetAlarmButtonInteractable(false);
            }
            if (other.GetComponent<VotingZone>() != null)
            {
                _skills.SetAlarmButtonInteractable(false);
            }

            if (other.GetComponent<MinigameZone>() != null)
            {
                _skills.SetInteractButtonInteractable(false);
                _minigamesManager.CurrentMinigameID = -1;
            }
            var Domofon = other.GetComponent<DomofonZone>();
            if (Domofon != null && _Controller.isImposter)
            {
                CurrentDomofon = null;
                _skills.SetDomofonButtonInteracteble(false);
            }
            var sczone = other.GetComponent<ShortCutZone>();
            if (sczone != null && _Controller.isImposter)
            {
                _skills.SetShortCutButtonInteractable(false);
                myShortCutZone = null;
            }
        }

        public ShortCutZone myShortCutZone;

        public DomofonZone GetDomofon()
        {
            return CurrentDomofon;
        }

        public void HideDeadBody()
        {
            HiddenDeadBody = true;
            Hide();
        }

        /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(IsInShortCut);
            }

            if (stream.IsReading)
            {
                IsInShortCut = (bool)stream.ReceiveNext();
            }
        }*/
    } 
}

