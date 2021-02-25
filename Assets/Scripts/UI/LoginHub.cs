using System;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Random = UnityEngine.Random;

public class LoginHub : MonoBehaviourPunCallbacks
{
    [Header("Login Panel")] public GameObject LoginPanel;

    public InputField PlayerNameInput;

    [Header("Selection Panel")] public GameObject SelectionPanel;

    [Header("Create Room Panel")] public GameObject CreateRoomPanel;

    public InputField RoomNameInputField;
    public InputField MaxPlayersInputField;
    private int MaxPlayers
    {
        get
        {
            int.TryParse(MaxPlayersInputField.text,out var result);
            return result;
        }
        set => MaxPlayersInputField.text = Mathf.Clamp(value,Killers*3+1,10).ToString();
    }
    public InputField KillersAmountInputField;
    private int Killers
    {
        get
        {
            int.TryParse(KillersAmountInputField.text,out var result);
            return result;
        }
        set => KillersAmountInputField.text = Mathf.Clamp(value,1,3).ToString();
    }

    [Header("Join Random Room Panel")] public GameObject JoinRandomRoomPanel;

    [Header("Room List Panel")] public GameObject RoomListPanel;
    public GameObject RoomListContent;
    public GameObject RoomListEntryPrefab;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;

    #region UNITY

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.SendRate = 18;
        PhotonNetwork.GameVersion = Application.version; 
        PhotonNetwork.SerializationRate = 18;
        PlayerPrefs.SetInt("isRandomLevel", 0);
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();
        RoomNameInputField.text = "Room " + Random.Range(1000, 10000);

        if (!PlayerPrefs.HasKey("NickName"))
        {
            PlayerNameInput.text = "Player " + Random.Range(1000, 10000);
        }
        else
        {
            PlayerNameInput.text = PlayerPrefs.GetString("NickName");
        }

        if (PlayerPrefs.HasKey("NickName") && PhotonNetwork.IsConnectedAndReady)
        {
            LoginPanel.SetActive(false);
            SelectionPanel.SetActive(true);    
        }
        
    }

    #endregion

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        SetActivePanel(SelectionPanel.name);
        var hashtable = new Hashtable {["SelectedSkin"] = PlayerPrefs.GetInt("SelectedSkin"),["LocalNumber"] = -1};
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();

        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(SelectionPanel.name);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(SelectionPanel.name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room " + Random.Range(1000, 10000);

        PlayerPrefs.SetInt("HostAmountOfKillers",1);
        RoomOptions options = new RoomOptions {MaxPlayers = 8};

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    #endregion

    #region UI CALLBACKS

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        SetActivePanel(SelectionPanel.name);
    }

    public void OnCreateRoomButtonClicked()
    {
        var roomName = RoomNameInputField.text;
        roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

        byte.TryParse(MaxPlayersInputField.text, out var maxPlayers);
        int.TryParse(KillersAmountInputField.text, out var AmountOfKillers);

        PlayerPrefs.SetInt("HostAmountOfKillers",AmountOfKillers);
        var options = new RoomOptions {MaxPlayers = maxPlayers, PlayerTtl = 1000};
        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        SetActivePanel(JoinRandomRoomPanel.name);
        PlayerPrefs.SetInt("isRandomLevel", 1);
        PhotonNetwork.JoinRandomRoom();
    }

    public void Disconnect()
    {
        PlayerPrefs.DeleteKey("NickName");
        PhotonNetwork.Disconnect();
    }
    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;
        if (!playerName.Equals(""))
        {
            PlayerPrefs.SetString("NickName",playerName);
            PhotonNetwork.LocalPlayer.NickName = playerName;
            if (PhotonNetwork.IsConnected)
            {
                LoginPanel.SetActive(false);
                SelectionPanel.SetActive(true);
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }

    public void OnRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        SetActivePanel(RoomListPanel.name);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    #endregion

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    private void SetActivePanel(string activePanel)
    {
        LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
        SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
        CreateRoomPanel.SetActive(activePanel.Equals(CreateRoomPanel.name));
        JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));
        RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name)); // UI should call OnRoomListButtonClicked() to activate this
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte) info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsConnected && PlayerPrefs.HasKey("NickName"))
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.SendRate = 16;
            PhotonNetwork.GameVersion = Application.version; 
            PhotonNetwork.SerializationRate = 16;
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void IncreaseKillersLevel()
    {
        Killers++;
        MaxPlayers += 0;
    }

    public void DecreaseKillersLevel()
    {
        Killers--;
        MaxPlayers += 0;
    }

    public void IncreasePlayersLevel()
    {
        MaxPlayers++;
    }

    public void DecreasePlayersLevel()
    {
        MaxPlayers--;
    }
}
