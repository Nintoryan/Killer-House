using UnityEngine;

namespace AAPlayer
{
    public class Skills : MonoBehaviour
    {
        [SerializeField] private KillingZone _killingZone;

        public void TryKill()
        {
            var victim = _killingZone.GetPlayer();
            if (victim != null)
            {
                _killingZone.PlayersInside.Remove(victim._Body);
                Controller.Die(victim);
            }
        }
    }
}

