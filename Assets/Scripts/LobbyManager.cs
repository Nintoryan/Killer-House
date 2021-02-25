using System.Collections.Generic;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public Button StartGameButton;
    [SerializeField] private TMP_Text AmountOfPlayers;

    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        var s = DOTween.Sequence();
        s.AppendInterval(1.25f);
        s.AppendCallback(() =>
        {
            int playersAmount = System.Convert.ToInt32(PhotonNetwork.CurrentRoom.PlayerCount);
            var NumbersList = new List<int>();
            for (int i = 0; i < playersAmount; i++)
            {
                NumbersList.Add(i);
            }
            int ImposterID1 = Random.Range(0, playersAmount);
            NumbersList.Remove(ImposterID1);
            int ImposterID2 = NumbersList[Random.Range(0, NumbersList.Count)];
            NumbersList.Remove(ImposterID2);
            int ImposterID3 = NumbersList[Random.Range(0, NumbersList.Count)];
            NumbersList.Remove(ImposterID3);
            Debug.Log($"Players Amount:{PhotonNetwork.CurrentRoom.PlayerCount}");
            Debug.Log($"Imposter:{ImposterID1},{ImposterID2},{ImposterID3}");
            var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            var sendOptions = new SendOptions {Reliability = true};
            var data = new object[]
            {
                ImposterID1,ImposterID2,ImposterID3
            };
            PhotonNetwork.RaiseEvent(43, data, options, sendOptions);
            AmountOfPlayers.gameObject.SetActive(false);
        });
    }

    private void Awake()
    {
        StartGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
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
        var gm = GameManager.Instance;
        foreach (var player in gm._players)
        {
            if (Equals(PhotonNetwork.LocalPlayer, player._photonView.Owner))
            {
                return;
            }
        }
        PhotonNetwork.Instantiate(PlayerPrefab.name, 
            gm.SpawnPlaces[Random.Range(0,gm.SpawnPlaces.Length)].position, 
            Quaternion.identity);
    }

    public override void OnLeftRoom()
    {
        if (PlayerPrefs.GetInt("ToEndGameScreen") == 1)
        {
            PlayerPrefs.SetInt("ToEndGameScreen",0);
            SceneManager.LoadScene("EndGameScreen");
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        var metrica = AppMetrica.Instance;
        var paramerts = new Dictionary<string, object>
        {
            {"level_number", 1},
            {"level_count", PlayerPrefs.GetInt("levelNumber")},
            {"result", "leave"},
            {"time", (int)GameManager.Instance.TimeSinceGameStarted},
            {"progress",0}
        };
        metrica.ReportEvent("level_finish",paramerts);
        metrica.SendEventsBuffer();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        StartGameButton.gameObject.SetActive(PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber &&
                                             PhotonNetwork.CurrentRoom.IsOpen);
        StartGameButton.interactable = CheckPlayersReady();
    }

    private bool CheckPlayersReady()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount >= GameManager.Instance.AmountOfKillers * 3 + 1;
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