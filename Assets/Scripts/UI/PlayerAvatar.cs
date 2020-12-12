using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Voting
{
    public class PlayerAvatar : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        public Image WhoStartedIcon;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _nickName;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Image _cross;
        public Image Voted;
        public Image Skiped;
        [SerializeField] private Image[] VoitingPortraits;
        [SerializeField] private Sprite[] allIcons;
        public PlayerAvatar _suspectPlayer;
        public GameObject WhoVotedParent;

        private List<int> _suspectedByPlayersIDs = new List<int>();
        
        //public PlayerAvatar _protectedPlayer;
        public void AddToSuspectedByPlayer(int id)
        {
            if (!_suspectedByPlayersIDs.Contains(id))
            {
                _suspectedByPlayersIDs.Add(id);
                VoitingPortraits[id].gameObject.SetActive(true);
            }
        }
        public void RemoveFromSuspectedByPlayer(int id)
        {
            if (_suspectedByPlayersIDs.Contains(id))
            {
                _suspectedByPlayersIDs.Remove(id);
                VoitingPortraits[id].gameObject.SetActive(false);
            }
        }

        private int _kickScore;
        public int KickScore
        {
            get => _suspectedByPlayersIDs.Count;
        }
        public int thisPlayerActorID;
        public int localPlayerNumber;

        public void Initialize(string NickName, int skinID, int ActorID)
        {
            _icon.sprite = allIcons[skinID];
            thisPlayerActorID = ActorID;
            _nickName.text = NickName;
            Voted.gameObject.SetActive(false);
            Skiped.gameObject.SetActive(false);
            _suspectedByPlayersIDs.Clear();
            foreach (var t in VoitingPortraits)
            {
                t.gameObject.SetActive(false);
            }
            _kickScore = 0;
            if (GameManager.Instance.FindPlayer(ActorID).IsDead)
            {
                _cross.gameObject.SetActive(true);
                _button.interactable = false;
            }
        }
    }

}
