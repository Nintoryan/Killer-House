using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Asteroids
{
    public class LobbyTopPanel : MonoBehaviour
    {
        private readonly string connectionStatusMessage = "    Connection Status: ";

        [Header("UI References")]
        public Text ConnectionStatusText;

        public Text AmountOfPlayersOnline;

        #region UNITY

        public void Update()
        {
            ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
            if(AmountOfPlayersOnline == null) return;
            AmountOfPlayersOnline.text = PhotonNetwork.CountOfPlayers != 0 ? $"Now {PhotonNetwork.CountOfPlayers} players online" : "";
        }

        #endregion
    }
}