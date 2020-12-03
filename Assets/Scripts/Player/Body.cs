﻿using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
namespace AAPlayer
{
    public class Body : MonoBehaviour
    {
        public Controller _Controller;
        [SerializeField] private Skills _skills;
        [SerializeField] private GameObject _Graphics;
        private List<Controller> _deadBodies = new List<Controller>();
        public bool HiddenDeadBody;

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
                _skills.EnterDeadBody();
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
                _skills.ExitDeadBody();
            }
        }

        public void HideDeadBody()
        {
            HiddenDeadBody = true;
            Hide();
        }
    } 
}

