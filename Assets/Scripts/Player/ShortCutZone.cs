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

    public void Use(){
        
        Debug.Log($"Домофон был использован номер:{Number}");
        var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        var sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(75, Number, options, sendOptions);
        var s = DOTween.Sequence();

    }
    public void GetEventUse()
    {
        _animator.SetTrigger("activate");
        var s = DOTween.Sequence();
        s.AppendInterval(1f);
        s.AppendCallback(() =>
        {
            _animator.ResetTrigger("activate");
        });
    }
}
