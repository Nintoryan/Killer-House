using UnityEngine;

namespace AAPlayer
{
    public class Body : MonoBehaviour
    {
        public Controller _Controller;
        [SerializeField] private GameObject _Graphics;

        private void Hide()
        {
            _Graphics.SetActive(false);
        }

        private void Show()
        {
            _Graphics.SetActive(true);
        }

        private void FixedUpdate()
        {
            if (!_Controller._photonView.IsMine) return;
            foreach (var player in Controller.AllPlayers)
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
    } 
}

