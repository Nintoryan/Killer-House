using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ShortCutZone : MonoBehaviour
{
    public int Number;
    public Transform PlayerInSidePosition;
    [SerializeField] private Animator _animator;

    public void Use(int KillerLocalNumber){
        
        Debug.Log($"Дверь была использована:{Number}. Киллером: KillerLocalNumber");
        var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        var sendOptions = new SendOptions {Reliability = true};
        var data = new object[]
        {
            Number,KillerLocalNumber
        };
        PhotonNetwork.RaiseEvent(75, data, options, sendOptions);
        var s = DOTween.Sequence();

    }
    public void GetEventUse(int KillerLocalNumber)
    {
        _animator.SetTrigger("activate");
        var gm = GameManager.Instance;
        if (!gm.LocalPlayer.isImposter)
        {
            var KillerThatUsed = gm.FindPlayer(KillerLocalNumber);
            bool isActive = KillerThatUsed._Body.gameObject.activeInHierarchy;
            KillerThatUsed._Body.gameObject.SetActive(!isActive);
            KillerThatUsed.NickNameCanvas.gameObject.SetActive(!isActive);
        }
        var s = DOTween.Sequence();
        s.AppendInterval(1f);
        s.AppendCallback(() =>
        {
            _animator.ResetTrigger("activate");
        });
    }
}
