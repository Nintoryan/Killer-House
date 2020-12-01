using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using AAPlayer;

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


        public void StartGame()
        {
            RaiseEventOptions options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(43, Random.Range(0,playerListEntries.Count), options, sendOptions);
        }
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
            PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(Random.Range(15, 25), -1.7f, Random.Range(-5, 5)), Quaternion.identity);
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
                PlayersHub.GetChild(i).GetComponent<Controller>()._skin.UpdateColor(i);
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
                    case 0: return new Color(0.1f, 0.74f, 0.61f);
                    case 1: return new Color(0.18f, 0.8f, 0.44f);
                    case 2: return new Color(0.2f, 0.6f, 0.86f);
                    case 3: return new Color(0.61f, 0.35f, 0.71f);
                    case 4: return new Color(0.2f, 0.29f, 0.37f);
                    case 5: return new Color(0.93f, 0.94f, 0.95f);
                    case 6: return new Color(0.91f, 0.3f, 0.24f);
                    case 7: return new Color(0.83f, 0.33f, 0f);
                    case 8: return new Color(0.95f, 0.77f, 0.06f);
                    case 9: return new Color(0.95f, 0.61f, 0.07f);
                    default:return new Color(Random.Range(0,1f),Random.Range(0,1f),Random.Range(0,1f),1);
                }
        }
    }