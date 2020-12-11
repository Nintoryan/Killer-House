using System.Collections.Generic;
using AAPlayer;
using UnityEngine;

public class DyingZone : MonoBehaviour
{
    private List<Controller> _controllers = new List<Controller>();

    private void OnTriggerEnter(Collider other)
    {
        var body = other.GetComponent<Body>();
        Controller player = null;
        if (!(body is null))
        {
            player = body._Controller;
        }
        if (player != null)
        {
            if(!_controllers.Contains(player))
            {
                _controllers.Add(player);
                player.DieEvent();
            }
        }
    }
}
