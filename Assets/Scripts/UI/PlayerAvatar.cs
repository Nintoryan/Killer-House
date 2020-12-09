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
        public List<int> suspectedByPlayersID;
        //public PlayerAvatar _protectedPlayer;

        private int _kickScore;
        public int KickScore
        {
            get => _kickScore;
            set
            {
                _kickScore = value;
                _scoreText.text = $"{value}";
                for (int i = 0; i < VoitingPortraits.Length; i++)
                {
                    VoitingPortraits[i].gameObject.SetActive(suspectedByPlayersID.Contains(i));
                }
            }
        }
        public int thisPlayerActorID;
        public int localPlayerNumber;

        public void Initialize(string NickName, int skinID, int ActorID)
        {
            _icon.sprite = allIcons[skinID];
            thisPlayerActorID = ActorID;
            _nickName.text = NickName;
            if (GameManager.Instance.FindPlayer(ActorID).IsDead)
            {
                _cross.gameObject.SetActive(true);
                _button.interactable = false;
            }
        }
    }

}
