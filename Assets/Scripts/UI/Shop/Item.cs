using System;
using TMPro;
using UnityEngine;
using UserData;

namespace Shop
{
    public class Item : MonoBehaviour
    {
        public int PPID;
        public int Cost;
        public Currency _currency;
        public Type _type;
        [SerializeField] private GameObject PriceSign;
        [SerializeField] private TMP_Text Price;
        [SerializeField] private GameObject SelectingOutline;
        [SerializeField] private GameObject DefaultOutline;
        [SerializeField] private GameObject Selected;

        private State _state;

        public int StatePPValue
        {
            get => PlayerPrefs.GetInt($"{_type}{PPID}");
            set
            {
                PlayerPrefs.SetInt($"{_type}{PPID}", value);
                if (value == 2)
                {
                    PlayerPrefs.SetInt($"Selected{_type}",PPID);
                }
            }
        }

        public State CurrentState => (State)StatePPValue;

        private void Start()
        {
            if (Currency.Wins == _currency && Statistics.Wins >= Cost)
            {
                StatePPValue = 1;
            }
            Refresh();
            if (CurrentState == State.Selected)
            {
                switch (_type)
                {
                    case Type.Skin:
                        ShopPreview.Instance.SelectedSkinItem = this;
                        break;
                    case Type.Dance:
                        ShopPreview.Instance.SelectedDanceItem = this;
                        break;
                }
                
            }
        }

        public void Refresh()
        {
            switch (CurrentState)
            {
                case State.Locked:
                    PriceSign.SetActive(true);
                    Price.text = Cost.ToString();
                    break;
                case State.Unlocked:
                    PriceSign.SetActive(false);
                    DefaultOutline.SetActive(true);
                    SelectingOutline.SetActive(false);
                    Selected.SetActive(false);
                    break;
                case State.Selected:
                    PriceSign.SetActive(false);
                    DefaultOutline.SetActive(true);
                    SelectingOutline.SetActive(false);
                    Selected.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void Select()
        {
            ShopPreview.Instance.Select(this);
            
            switch (CurrentState)
            {
                case State.Locked:
                    DefaultOutline.SetActive(false);
                    SelectingOutline.SetActive(true);
                    break;
                case State.Unlocked:
                    StatePPValue = 2;
                    Refresh();
                    switch (_type)
                    {
                        case Type.Skin:
                            ShopPreview.Instance.SelectedSkinItem = this;
                            break;
                        case Type.Dance:
                            ShopPreview.Instance.SelectedDanceItem = this;
                            break;
                    }
                    break;
            }
            
        }

        public void Deselect()
        {
            DefaultOutline.SetActive(true);
            SelectingOutline.SetActive(false);
        }
    }

    [Serializable]
    public enum State
    {
        Locked = 0,
        Unlocked = 1,
        Selected = 2
    } 
    [Serializable]
    public enum Currency
    {
        Money = 0,
        Wins = 1
    }    
    [Serializable]
    public enum Type
    {
        Skin = 0,
        Dance = 1
    }   
}
