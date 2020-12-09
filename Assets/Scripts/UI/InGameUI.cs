using Photon.Pun;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private GameObject[] QuestMarks;
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void SetMarkActive(int MarkID)
    {
        QuestMarks[MarkID].SetActive(true);
    }

    public void SetMarkDisables(int MarkID)
    {
        QuestMarks[MarkID].SetActive(false);
    }
    
}
