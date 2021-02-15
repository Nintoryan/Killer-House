using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private Transform PlayerLogParent;
    [SerializeField] private TMP_Text PlayerNick;

    public void ShowPlayerJoinLeave(string a)
    {
        var inst = Instantiate(PlayerNick.gameObject, PlayerLogParent);
        inst.GetComponent<TMP_Text>().text = a;
        var s = DOTween.Sequence();
        s.AppendInterval(3f);
        s.AppendCallback(() =>
        {
            Destroy(inst);
        });
    }
    public void Leave(string result = "exit")
    {
        if (GameManager.Instance.isGameStarted)
        {
            var metrica = AppMetrica.Instance;
            var paramerts = new Dictionary<string, object>
            {
                {"level", PlayerPrefs.GetInt("levelNumber")},
                {"result", result},
                {"time", GameManager.Instance.TimeSinceGameStarted},
                {"progress",result == "exit"?0:GameManager.Instance.Progress}
            };
            metrica.ReportEvent("level_finish",paramerts);
            metrica.SendEventsBuffer();
            if (result != "exit")
            {
                PlayerPrefs.SetInt("ToEndGameScreen",1);
            }
        }
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
