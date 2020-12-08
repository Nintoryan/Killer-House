using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
namespace AAPlayer
{
    public class Body : MonoBehaviour
    {
        public Controller _Controller;
        [SerializeField] private Skills _skills;
        [SerializeField] private GameObject _Graphics;
        [SerializeField] private GameObject[] AllSkins;
        [SerializeField] private MinigamesManager _minigamesManager;
        private List<Controller> _deadBodies = new List<Controller>();
        public bool HiddenDeadBody;

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
        private void Hide()
        {
            _Graphics.SetActive(false);
            _Controller.DisableNickName();
        }

        private void Show()
        {
            if(HiddenDeadBody) return;
            _Graphics.SetActive(true);
            _Controller.ActivateNickName();
        }

        private void FixedUpdate()
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
                _skills.EnableAlarmButton();
            }

            if (other.GetComponent<VotingZone>() != null && !_skills.HadSpawnAlarm)
            {
                _skills.EnableAlarmButton();
            }
            var MiniGame = other.GetComponent<MinigameZone>();
            if (MiniGame != null)
            {
                if (!MiniGame.isComplete)
                {
                    _skills._interactButton.interactable = true;
                    _minigamesManager.CurrentMinigameID = MiniGame.Number;
                }
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
                _skills.DisableAlarmButton();
            }
            if (other.GetComponent<VotingZone>() != null)
            {
                _skills.DisableAlarmButton();
            }

            if (other.GetComponent<MinigameZone>() != null)
            {
                _skills._interactButton.interactable = false;
                _minigamesManager.CurrentMinigameID = -1;
            }
        }

        public void HideDeadBody()
        {
            HiddenDeadBody = true;
            Hide();
        }
    } 
}

