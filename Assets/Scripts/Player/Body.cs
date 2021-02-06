using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Body : MonoBehaviour//,IPunObservable
    {
        public Controller _Controller;
        [SerializeField] private Skills _skills;
        [SerializeField] private GameObject _Graphics;
        [SerializeField] private GameObject _AliveGraphics;
        [SerializeField] private GameObject _GhostGraphics;
        [FormerlySerializedAs("AllSkins")] [SerializeField] private GameObject[] Skins;
        [SerializeField] private GameObject[] Ghosts;
        [SerializeField] private MinigamesManager _minigamesManager;
        private List<Controller> _deadBodies = new List<Controller>();
        public bool HiddenDeadBody;
        //public bool IsInShortCut;

        private DomofonZone CurrentDomofon;

        public Animator _Animator => _Graphics.GetComponent<Animator>();
        
        public void Initialize(int id)
        {
            foreach (var t in Skins)
            {
                t.SetActive(false);
            }
            Skins[id].SetActive(true);
            _Graphics = Skins[id];
            _AliveGraphics = Skins[id];
            _GhostGraphics = Ghosts[id];
        }

        public void SwitchToDead()
        {
            _Graphics = _GhostGraphics;
            _AliveGraphics.transform.SetParent(null);
            if (GameManager.Instance.LocalPlayer.IsDead)
            {
                _GhostGraphics.SetActive(true);
                _GhostGraphics.transform.position = _AliveGraphics.transform.position;
            }
        }
        public void Hide()
        {
            _AliveGraphics.SetActive(false);
            if (!_Controller._photonView.IsMine)
            {
                _Controller.DisableNickName();
            }
        }

        public void Show()
        {
            if(HiddenDeadBody) return;
            _Graphics.SetActive(true);
            _Controller.ActivateNickName();
        }
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

        public Transform AliveGraphicsTransform()
        {
            return _AliveGraphics.transform;
        }
        
    } 
}

