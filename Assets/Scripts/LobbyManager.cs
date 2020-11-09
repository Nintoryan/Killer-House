using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public GameObject PlayerPrefab;
        [Header("Inside Room Panel")] 
        
        public GameObject InsideRoomPanel;
        public Button StartGameButton;
        public GameObject PlayerListEntryPrefab;
        public Transform PlayersHub;

        private Dictionary<int, GameObject> playerListEntries;
        private bool isHide;


        public void ShowHideInsideRoomPanel()
        {
            InsideRoomPanel.SetActive(isHide);
            isHide = !isHide;
        }
        public void Leave()
        {
            PhotonNetwork.LeaveRoom();
        }

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                OnJoinedRoom();
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            var entry = Instantiate(PlayerListEntryPrefab, InsideRoomPanel.transform, true);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);
            playerListEntries.Add(newPlayer.ActorNumber, entry);
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnJoinedRoom()
        {
            SetActivePanel(InsideRoomPanel.name);
            var Character = PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(Random.Range(10, 15), -1.7f, Random.Range(-5, 0)), Quaternion.identity);
            Character.transform.SetParent(PlayersHub);
            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }
            foreach (var p in PhotonNetwork.PlayerList)
            {
                var entry = Instantiate(PlayerListEntryPrefab, InsideRoomPanel.transform, true);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<PlayerListEntry>().Initialize(p.ActorNumber, p.NickName);

                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out var isPlayerReady))
                {
                    entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool) isPlayerReady);
                }
                playerListEntries.Add(p.ActorNumber, entry);
            }

            for (int i = 0; i < PlayersHub.childCount; i++)
            {
                PlayersHub.GetChild(i).GetComponent<PlayerController>().UpdateColor(i);
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());

            var props = new Hashtable
            {
                {AsteroidsGame.PLAYER_LOADED_LEVEL, false}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                StartGameButton.gameObject.SetActive(CheckPlayersReady());
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out var entry))
            {
                if (changedProps.TryGetValue(AsteroidsGame.PLAYER_READY, out var isPlayerReady))
                {
                    entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool) isPlayerReady);
                }
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public void LocalPlayerPropertiesUpdated()
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        private void SetActivePanel(string activePanel)
        {
            InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
        }

        private bool CheckPlayersReady()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }

            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out var isPlayerReady))
                {
                    if (!(bool) isPlayerReady)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public static Color GetColor(int id)
        {
            switch (id)
                {
                    case 0: return Color.red;
                    case 1: return Color.green;
                    case 2: return Color.blue;
                    case 3: return Color.yellow;
                    case 4: return Color.cyan;
                    case 5: return Color.grey;
                    case 6: return Color.magenta;
                    case 7: return Color.white;
                }

                return Color.black;
        }
    }