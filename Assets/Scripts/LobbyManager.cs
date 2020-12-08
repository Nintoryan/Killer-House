using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public Button StartGameButton;
    [SerializeField] private TMP_Text AmountOfPlayers;

    public void StartGame()
    {
        int ImposterID = Random.Range(0, System.Convert.ToInt32(PhotonNetwork.CurrentRoom.PlayerCount));
        Debug.Log($"Players Amount:{PhotonNetwork.CurrentRoom.PlayerCount}");
        Debug.Log($"Imposter:{ImposterID}");
        RaiseEventOptions options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        SendOptions sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(43, ImposterID, options, sendOptions);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        AmountOfPlayers.gameObject.SetActive(false);
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

        AmountOfPlayers.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AmountOfPlayers.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        StartGameButton.interactable = CheckPlayersReady();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        AmountOfPlayers.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.IsOpen);
        StartGameButton.interactable = CheckPlayersReady();
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(Random.Range(15, 25), -1.7f, Random.Range(-5, 5)), Quaternion.identity);
        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        StartGameButton.interactable = CheckPlayersReady();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        StartGameButton.gameObject.SetActive(PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber &&
                                             PhotonNetwork.CurrentRoom.IsOpen);
        StartGameButton.interactable = CheckPlayersReady();
    }

    private bool CheckPlayersReady()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public static Color GetColor(int id)
    {
        switch(id)
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
            default: return new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1);
        };
    }
}