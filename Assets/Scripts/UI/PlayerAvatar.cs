using MPUIKIT;
using TMPro;
using UnityEngine;

namespace Voting
{
    public class PlayerAvatar : MonoBehaviour
    {
        [SerializeField] private MPImage _icon;
        [SerializeField] private TMP_Text _nickName;
        public int ForAmount;
        public int AgainstAmount;
        public int KickScore => AgainstAmount - ForAmount;
        public int ID;
        public Vector2 RectPositionIncoming => GetComponent<RectTransform>().anchoredPosition + new Vector2(10,10);
        public Vector2 RectPositionOutcoming => GetComponent<RectTransform>().anchoredPosition - new Vector2(10,10);

        public void Initialize(string NickName, Color color, int ActorID)
        {
            _icon.color = color;
            ID = ActorID;
            _nickName.text = NickName;
        }
    }

}
