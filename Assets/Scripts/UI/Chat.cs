using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Chat : MonoBehaviour, IChatClientListener
{
    private ChatClient _chatClient;
    [SerializeField] private string _userID;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _outputField;
    public GameObject Notifications;
    public GameObject ChatParent;
    private string _roomName;

    public static Chat DebugChatLog;


    private void Awake()
    {
        DebugChatLog = this;
    }

    public void Initialize(string userName,string roomName)
    {
        _chatClient = new ChatClient(this);
        _roomName = roomName;
        _chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion,
            new AuthenticationValues(userName));
        

    }

    private void Update()
    {
        _chatClient.Service();
    }

    public void Send()
    {
        if (_inputField.text != "")
        {
            _chatClient.PublishMessage(_roomName, _inputField.text);
            _inputField.text = "";
        }
    }

    public static void SendDebug(string message)
    {
        DebugChatLog._chatClient.PublishMessage(DebugChatLog._roomName, "<color=white>DEBUG:"+message+"</color>");
    }
    
    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnDisconnected()
    {
        _chatClient.Unsubscribe(new []{_roomName});
    }

    public void OnConnected()
    {
        _chatClient.Subscribe(_roomName);
    }

    public void OnChatStateChange(ChatState state)
    {
       Debug.Log("ChateStateChange.Invoked");
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (var i = 0; i < senders.Length; i++)
        {
            _outputField.text += $"<color=white>{senders[i]}</color>:{messages[i]}\n";
        }
        if (!ChatParent.activeInHierarchy)
        {
            Notifications.SetActive(true);
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        
    }

    public void OnUnsubscribed(string[] channels)
    {
        
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        
    }

    public void OnUserSubscribed(string channel, string user)
    {
       
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        
    }
}
