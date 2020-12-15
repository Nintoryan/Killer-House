﻿using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649
namespace AAPlayer
{
    public class KillingZone : MonoBehaviour
    {
        [SerializeField] private Body _myBody;
        public List<Body> PlayersInside = new List<Body>();

        private void OnTriggerEnter(Collider other)
        {
            
            var body = other.GetComponent<Body>();
            if (body && body != _myBody && !PlayersInside.Contains(body))
            {
                if (!body._Controller.IsDead)
                {
                    PlayersInside.Add(body);
                    if(!_myBody._Controller._photonView.IsMine) return;
                    _myBody._Controller._skills.SetKillingInteractable(true);
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            var body = other.GetComponent<Body>();
            if (body && body != _myBody && PlayersInside.Contains(body))
            {
                if (!body._Controller.IsDead)
                {
                    PlayersInside.Remove(body);
                    if (PlayersInside.Count == 0)
                    {
                        if(!_myBody._Controller._photonView.IsMine) return;
                        _myBody._Controller._skills.SetKillingInteractable(false);
                    }
                }
            }
        }

        public void TryRemoveDeadBody(Controller DeadPlayer)
        {
            var body = DeadPlayer._Body;
            if (PlayersInside.Contains(body))
            {
                PlayersInside.Remove(body);
            }
        }

        public Controller GetPlayer()
        {
            return PlayersInside.Count > 0 ? PlayersInside[0]._Controller : null;
        }
    }
}

