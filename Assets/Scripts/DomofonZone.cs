using DG.Tweening;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DomofonZone : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float ColDown = 30f;
    public int Number;
    private bool CanUse = true;
    public void Use(){
        if(!CanUse) return;
        Debug.Log($"Домофон был использован номер:{Number}");
        CanUse = false;
        var options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        var sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(67, Number, options, sendOptions);
        var s = DOTween.Sequence();
        s.AppendInterval(ColDown);
        s.AppendCallback(() =>
        {
            CanUse = true;
        });
    }

    public void GetEventUse()
    {
        _animator.SetTrigger("activate");
        Debug.Log($"Событие получено тупа стартуем номер:{Number}");
        var s = DOTween.Sequence();
        s.AppendInterval(1f);
        s.AppendCallback(() =>
        {
            _animator.ResetTrigger("activate");
        });
    }
}
