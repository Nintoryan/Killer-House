using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Skills : MonoBehaviour
    {
        [SerializeField] private KillingZone _killingZone;
        [SerializeField] private Button _killButton;
        [SerializeField] private Image _killButtonBg;
        [SerializeField] private int KillingColdown = 35;

        public void TryKill()
        {
            if (_killingZone.GetPlayer() != null)
            {
                _killingZone.GetPlayer().DieEvent();
                _killButton.interactable = false;
                _killButton.GetComponent<Image>().fillAmount = 0;
                var s = DOTween.Sequence();
                s.Append(_killButton.GetComponent<Image>().DOFillAmount(1, KillingColdown));
                s.AppendCallback(()=>
                {
                    _killButton.interactable = true;
                });
            }
        }

        public void DiableKilling()
        {
            _killButton.gameObject.SetActive(false);
            _killButtonBg.gameObject.SetActive(false);
        }
    }
}

