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
        [SerializeField] private Button _AlarmButton;
        [SerializeField] private Button domofonButton;
        [SerializeField] private Button _interactButton;
        [SerializeField] private int KillingColdown = 35;
        [SerializeField] private int DomofonColdown = 30;

        public bool HadSpawnAlarm;
        private int FoundBodyID = -1;
        private bool isKillOnCD = false;
        private bool isDomofonOnCD = false;

        public void TryKill()
        {
            if (_killingZone.GetPlayer() != null)
            {
                _killingZone.GetPlayer().DieEvent();
                KillGoCD();
            }
        }

        public void KillGoCD()
        {
            isKillOnCD = true;
            _killButton.interactable = false;
            _killButton.GetComponent<Image>().fillAmount = 0;
            _killButton.GetComponent<Image>().raycastTarget = false;
            var s = DOTween.Sequence();
            s.Append(_killButton.GetComponent<Image>().DOFillAmount(1, KillingColdown));
            s.AppendCallback(()=>
            {
                SetKillingInteractable(true);
                isKillOnCD = false;
            });
        }

        public void UseDomofon()
        {
            if (_body.GetDomofon() != null)
            {
                _body.GetDomofon().Use();
                isDomofonOnCD = true;
                Debug.Log($"Domofon:{_body.GetDomofon()}");
                SetDomofonButtonInteracteble(false);
                domofonButton.GetComponent<Image>().fillAmount = 0;
                var s = DOTween.Sequence();
                s.Append(domofonButton.GetComponent<Image>().DOFillAmount(1, DomofonColdown));
                s.AppendCallback(() =>
                {
                    SetDomofonButtonInteracteble(true);
                    isDomofonOnCD = false;
                });
            }
            else
            {
                Debug.Log("Нет домофона!");
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
                    SetAlarmButtonInteractable(false);
                }
            }
        }
        
        public void SetKillingActive(bool isActive)
        {
            _killButton.gameObject.SetActive(isActive);
        }

        public void SetKillingInteractable(bool isInteractable)
        {
            _killButton.interactable = isInteractable && !isKillOnCD;
            _killButton.GetComponent<Image>().raycastTarget = isInteractable && !isKillOnCD;
        }

        public void SetAlarmButtonActive(bool isActive)
        {
            _AlarmButton.gameObject.SetActive(isActive);
        }

        public void SetInteractButtonActive(bool isActive)
        {
            _interactButton.gameObject.SetActive(isActive);
        }

        public void SetInteractButtonInteractable(bool isInteractable)
        {
            _interactButton.interactable = isInteractable;
            _interactButton.GetComponent<Image>().raycastTarget = isInteractable;
        }
        
        public void SetAlarmButtonInteractable(bool isActive)
        {
            _AlarmButton.interactable = isActive;
            _AlarmButton.GetComponent<Image>().raycastTarget = isActive;
        }

        public void SetDomofonButtonActive(bool isActive)
        {
            domofonButton.gameObject.SetActive(isActive);
        }

        public void SetDomofonButtonInteracteble(bool isInteractable)
        {
            domofonButton.interactable = isInteractable && !isDomofonOnCD;
            domofonButton.GetComponent<Image>().raycastTarget = isInteractable;
        }
        
        
    }
}

