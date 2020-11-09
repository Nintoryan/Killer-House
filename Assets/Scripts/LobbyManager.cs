using System;
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

        private Dictionary<int, GameObject> playerListEntries;
        private Dictionary<int, GameObject> playerListCharacters;
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
            Debug.Log($"Player {newPlayer.NickName} connected to the room.");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);
            Debug.Log($"Player {otherPlayer.NickName} left the room.");
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnJoinedRoom()
        {
            SetActivePanel(InsideRoomPanel.name);
            PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(Random.Range(10, 15), -1.7f, Random.Range(-5, 0)), Quaternion.identity);
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

            GameObject entry;
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
            {
                object isPlayerReady;
                if (changedProps.TryGetValue(AsteroidsGame.PLAYER_READY, out isPlayerReady))
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

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object isPlayerReady;
                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out isPlayerReady))
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
    }