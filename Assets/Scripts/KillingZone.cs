﻿using System.Collections.Generic;
using UnityEngine;

namespace AAPlayer
{
    public class KillingZone : MonoBehaviour
    {
        [SerializeField] private Body _myBody;
        public List<Body> PlayersInside = new List<Body>();

        private void OnTriggerEnter(Collider other)
        {
            var body = other.GetComponent<Body>();
            if (body && body != _myBody)
            {
                PlayersInside.Add(body);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            var body = other.GetComponent<Body>();
            if (body && body != _myBody)
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

