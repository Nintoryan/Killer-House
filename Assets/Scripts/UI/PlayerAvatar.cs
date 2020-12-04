using MPUIKIT;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Voting
{
    public class PlayerAvatar : MonoBehaviour
    {
        [SerializeField] private MPImage _icon;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _nickName;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Image _cross;
        public PlayerAvatar _suspectPlayer;
        public PlayerAvatar _protectedPlayer;

        private int _kickScore;
        public int KickScore
        {
            get => _kickScore;
            set
            {
                _kickScore = value;
                _scoreText.text = $"{value}";
            }
        }
        public int ID;

        public void Initialize(string NickName, Color color, int ActorID)
        {
            _icon.color = color;
            ID = ActorID;
            _nickName.text = NickName;
            if (GameManager.Instance.FindPlayer(ActorID).IsDead)
            {
                _cross.gameObject.SetActive(true);
                _button.interactable = false;
            }
        }
    }

}
