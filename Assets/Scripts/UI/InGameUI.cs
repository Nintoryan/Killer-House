using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private GameObject[] QuestMarks;
    [SerializeField] private Slider ProgressBar;
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void SetProgress(float value)
    {
        ProgressBar.value = value;
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
