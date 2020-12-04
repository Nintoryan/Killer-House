using Voting;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Skills : MonoBehaviour
    {
        [SerializeField] private KillingZone _killingZone;
        [SerializeField] private Body _body;
        [SerializeField] private Button _killButton;
        [SerializeField] private Image _killButtonBg;
        [SerializeField] private Button _AlarmButton;
        [SerializeField] private int KillingColdown = 35;

        public bool HadSpawnAlarm;
        private int FoundBodyID = -1;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (GetComponent<Controller>()._photonView.IsMine
                    && _killButton.interactable
                    && _killButton.gameObject.activeInHierarchy)
                {
                    TryKill();
                }
            }
        }

        public void Alarm()
        {
            var Dead = _body.GetDeadBody();
            if (Dead != null)
            {
                Dead.DisableDeadBodyEvent();
                VotingManager.RaiseVotingEvent(GetComponent<Controller>());
            }
            else
            {
                if (!HadSpawnAlarm)
                {
                    HadSpawnAlarm = true;
                    VotingManager.RaiseVotingEvent(GetComponent<Controller>());
                    DisableAlarmButton();
                }
            }
        }

        public void DisableKilling()
        {
            _killButton.gameObject.SetActive(false);
            _killButtonBg.gameObject.SetActive(false);
        }
        public void EnableKilling()
        {
            _killButton.gameObject.SetActive(true);
            _killButtonBg.gameObject.SetActive(true);
        }

        public void HideAlarmButton()
        {
            _AlarmButton.gameObject.SetActive(false);
        }

        public void ShowAlarmButton()
        {
            _AlarmButton.gameObject.SetActive(true);
        }

        public void EnableAlarmButton()
        {
            _AlarmButton.interactable = true;
        }

        public void DisableAlarmButton()
        {
            _AlarmButton.interactable = false;
        }
        
    }
}

