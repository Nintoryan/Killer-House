using MPUIKIT;
using TMPro;
using UnityEngine;

namespace Voting
{
    public class PlayerAvatar : MonoBehaviour
    {
        [SerializeField] private MPImage _icon;
        [SerializeField] private TMP_Text _nickName;
        public PlayerAvatar _suspectPlayer;
        public PlayerAvatar _protectedPlayer;
        public int KickScore;
        public int ID;

        public void Initialize(string NickName, Color color, int ActorID)
        {
            _icon.color = color;
            ID = ActorID;
            _nickName.text = NickName;
        }
    }

}
