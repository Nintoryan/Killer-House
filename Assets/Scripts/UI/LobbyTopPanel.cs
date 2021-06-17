using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Asteroids
{
    public class LobbyTopPanel : MonoBehaviour
    {
        [Header("UI References")]
        public Text ConnectionStatusText;

        public Text AmountOfPlayersOnline;

        #region UNITY

        public void Update()
        {
            ConnectionStatusText.text = $"Connection Status:{PhotonNetwork.NetworkClientState}";
            if (PhotonNetwork.NetworkClientState == ClientState.Disconnected)
            {
                ConnectionStatusText.text = $"Connection Status:<color=red><b>{PhotonNetwork.NetworkClientState}</b></color>";
            }
            if(AmountOfPlayersOnline == null) return;
            AmountOfPlayersOnline.text = PhotonNetwork.CountOfPlayers != 0 ? $"Now {PhotonNetwork.CountOfPlayers + 7} players online" : "";
        }

        #endregion
    }
}