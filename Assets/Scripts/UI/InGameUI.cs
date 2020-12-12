using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private GameObject[] QuestMarks;
    [SerializeField] private Slider ProgressBar;
    [SerializeField] private RectTransform me;
    [SerializeField] private float a;
    [SerializeField] private float b;
    [SerializeField] private float a1;
    [SerializeField] private float b1;
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void MoveMe(Vector3 where)
    {
        me.anchoredPosition = new Vector2(
            where.x * a + b,
            where.z * a1 + b1);
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
