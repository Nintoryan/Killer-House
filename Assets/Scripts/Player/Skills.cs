using System;
using System.Collections.Generic;
using Voting;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UserData;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Skills : MonoBehaviour
    {
        [SerializeField] private KillingZone _killingZone;
        [SerializeField] private Body _body;
        [SerializeField] private Button _killButton;
        [SerializeField] private Button _watchTVButton;
        [SerializeField] private Button _AlarmButton;
        [SerializeField] private Button domofonButton;
        [SerializeField] private Button _interactButton;
        [SerializeField] private Button _EnterExitShortCut;
        public Button ChatButton;
        [SerializeField] private int KillingColdown = 35;
        [SerializeField] private int DomofonColdown = 30;
        [SerializeField] private Controller MyController;
        [Header("ShortCuts")] 
        [SerializeField] private ArrowsPack[] scArrows;

        [SerializeField] private GameObject JoyStick;
        
        private bool isInShortCut;
        public bool isDancing = false;

        public bool HadSpawnAlarm;
        private int FoundBodyID = -1;
        private bool isKillOnCD = false;
        private bool isDomofonOnCD = false;

        private int CurrentShortCutIn = -1;
        private static readonly int Status = Animator.StringToHash("status");


        public void DanceButton()
        {
            _body._Animator.SetInteger(Status,-2-PlayerPrefs.GetInt("SelectedDance"));
            isDancing = true;
        }
        private void EnterShortCut()
        {
            ShowArrows(_body.myShortCutZone.Number);
            CurrentShortCutIn = _body.myShortCutZone.Number;
            _body.myShortCutZone.Use(_body._Controller._photonView.Owner.ActorNumber);
            _body.Hide();
            JoyStick.SetActive(false);
        }

        private void ExitShortCut()
        {
            HideAllArrows();
            GameManager.Instance.AllShortCutZones[CurrentShortCutIn].Use(_body._Controller._photonView.Owner.ActorNumber);
            _body.Show();
            JoyStick.SetActive(true);
        }

        public void UseShortCutZone()
        {
            if (isInShortCut)
            {
                ExitShortCut();
            }
            else
            {
                EnterShortCut();
            }
            isInShortCut = !isInShortCut;
        }

        public void UseArrow(int id)
        {
            HideAllArrows();
            ShowArrows(id);
            CurrentShortCutIn = id;
            MyController.controller.enabled = false;
            _body.transform.position = GameManager.Instance.AllShortCutZones[id].PlayerInSidePosition.position;
            MyController.controller.enabled = true;
        }

        public void ShowArrows(int id)
        {
            foreach (var arrow in scArrows[id].Arrows)
            {
                arrow.gameObject.SetActive(true);
            }
        }

        private void HideAllArrows()
        {
            foreach (var t1 in scArrows)
            {
                foreach (var t in t1.Arrows)
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        public void TryKill()
        {
            if (_killingZone.GetPlayer() != null)
            {
                if(_killingZone.GetPlayer().isImposter) return;
                _killingZone.GetPlayer().DieEvent();
                _killingZone.TryRemoveDeadBody(_killingZone.GetPlayer());
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
                    GameManager.Instance.VotingSign.SetActive(false);
                    VotingManager.RaiseVotingEvent(GetComponent<Controller>());
                }
            }
            SetAlarmButtonInteractable(false);
        }
        
        public void SetKillingActive(bool isActive)
        {
            _killButton.gameObject.SetActive(isActive);
        }

        public void SetWatchTVButton(bool isActive)
        {
            _watchTVButton.gameObject.SetActive(isActive);
        }
        public void SetWatchTVInteractable(bool isActive)
        {
            _watchTVButton.interactable = isActive;
            _watchTVButton.GetComponent<Image>().raycastTarget = isActive;
        }

        public void WatchTV()
        {
            Advertisment.Instance.RewardedAd.OnReciveReward += () =>
            {
                Wallet.Balance += 150;
            };
            Advertisment.Instance.ShowRewarded("watchtv");
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

        public void SetShortCutButtonActive(bool isActive)
        {
            _EnterExitShortCut.gameObject.SetActive(isActive);
        }

        public void SetShortCutButtonInteractable(bool isInteractable)
        {
            _EnterExitShortCut.interactable = isInteractable;
            _EnterExitShortCut.GetComponent<Image>().raycastTarget = isInteractable;
        }
        
    }
}

[Serializable]
public class ArrowsPack
{
    public int Number;
    public List<Button> Arrows =new List<Button>();
}

