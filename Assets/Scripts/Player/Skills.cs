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
        [SerializeField] private Button _killButton;
        [SerializeField] private Image _killButtonBg;
        [SerializeField] private Button _AlarmButton;
        [SerializeField] private int KillingColdown = 35;
        [SerializeField] private VotingManager _votingManager;

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
            if (FoundBodyID != -1)
            {
                _votingManager.VotingEventDeadbody(FoundBodyID);
            }
            else
            {
                _votingManager.VotingEventMeeting();
            }
        }

        public void DiableKilling()
        {
            _killButton.gameObject.SetActive(false);
            _killButtonBg.gameObject.SetActive(false);
        }
        public void EnableKilling()
        {
            _killButton.gameObject.SetActive(true);
            _killButtonBg.gameObject.SetActive(true);
        }

        public void EnterDeadBody(int id)
        {
            _AlarmButton.interactable = true;
            FoundBodyID = id;
        }

        public void ExitDeadBody()
        {
            _AlarmButton.interactable = false;
            FoundBodyID = -1;
        }
        
    }
}

