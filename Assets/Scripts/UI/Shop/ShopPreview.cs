using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UserData;

namespace Shop
{
    public class ShopPreview : MonoBehaviour
    {
        [SerializeField] private GameObject[] AllSkins;
        [SerializeField] private GameObject AddMoneypPanel;
        private GameObject CurrentSkin;
        [SerializeField] private Button BuyButton;
        [SerializeField] private Image BuyButtonCurrency;
        [SerializeField] private GameObject SelectedButton;
        [SerializeField] private Sprite Skull;
        [SerializeField] private Sprite Key;
        [SerializeField] private TMP_Text Price;
        private Item CurrentItem;
        private Item CurrentSkinItem;
        public Item SelectedSkinItem;
        private Item CurrentDanceItem;
        public Item SelectedDanceItem;
        public static ShopPreview Instance;
        private static readonly int Status = Animator.StringToHash("status");

        public event UnityAction OnBuy; 

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            Refresh();
            BuyButton.gameObject.SetActive(false);
        }

        private void Refresh()
        {
            if (CurrentSkin != null)
            {
                CurrentSkin.SetActive(false);
            }
            CurrentSkin = AllSkins[PlayerPrefs.GetInt($"Selected{Type.Skin}")];
            CurrentSkin.SetActive(true);
            CurrentSkin.GetComponent<Animator>().SetInteger(Status,-2-PlayerPrefs.GetInt($"Selected{Type.Dance}"));
        }

        public void Select(Item _item)
        {
            CurrentItem = _item;
            switch (_item.CurrentState)
            {
                case State.Locked:
                    SelectedButton.SetActive(false);
                    Price.text = _item.Cost.ToString();
                    BuyButton.gameObject.SetActive(true);
                    Change(_item);
                    
                    if (_item._currency == Currency.Money)
                    {
                        BuyButtonCurrency.sprite = Skull;
                        BuyButton.interactable = true;
                    }
                    else
                    {
                        BuyButton.interactable = Wallet.Keys >= _item.Cost;
                        BuyButtonCurrency.sprite = Key;
                    }
                    break;
                case State.Unlocked:
                    Change(_item);
                    _item.StatePPValue = 2;
                    SelectedButton.SetActive(true);
                    _item.Refresh();
                    switch (_item._type)
                    {
                        case Type.Skin:
                            if (SelectedSkinItem != null)
                            {
                                SelectedSkinItem.StatePPValue = 1;
                                SelectedSkinItem.Refresh(); 
                            }
                            SelectedSkinItem = _item;
                            break;
                        case Type.Dance:
                            if (SelectedDanceItem != null)
                            {
                                SelectedDanceItem.StatePPValue = 1;
                                SelectedDanceItem.Refresh(); 
                            }
                            SelectedDanceItem = _item;
                            break;
                    }
                    break;
                case State.Selected:
                    SelectedButton.SetActive(true);
                    break;
            }
            switch (_item._type)
            {
                case Type.Skin:
                    if (CurrentSkinItem != null)
                    {
                        CurrentSkinItem.Deselect();
                    }
                    CurrentSkinItem = _item;
                    break;
                case Type.Dance:
                    if (CurrentDanceItem != null)
                    {
                        CurrentDanceItem.Deselect();
                    }
                    CurrentDanceItem = _item;
                    break;
            }
        }

        private void Change(Item _item)
        {
            switch (_item._type)
            {
                case Type.Skin:
                    CurrentSkin.SetActive(false);
                    CurrentSkin = AllSkins[_item.PPID];
                    CurrentSkin.SetActive(true);
                    CurrentSkin.GetComponent<Animator>().SetInteger(Status,-2-PlayerPrefs.GetInt($"Selected{Type.Dance}"));
                    break;
                case Type.Dance:
                    CurrentSkin.GetComponent<Animator>().SetInteger(Status,-2-_item.PPID);
                    break;
            }
        }

        public void Buy()
        {
            switch (CurrentItem._type)
            {
                case Type.Skin:
                    BuyCustom(CurrentSkinItem,ref SelectedSkinItem);
                    break;
                case Type.Dance:
                    BuyCustom(CurrentDanceItem,ref SelectedDanceItem);
                    break;
            }
        }

        private void BuyCustom(Item currentItem,ref Item selectedItem)
        {
            switch (currentItem._currency)
            {
                case Currency.Money when Wallet.Balance >= currentItem.Cost:
                    Wallet.Balance -= currentItem.Cost;
                    break;
                case Currency.Money:
                    AddMoneypPanel.SetActive(true);
                    return;
                case Currency.Wins when Wallet.Keys >= currentItem.Cost:
                    Wallet.Keys -= currentItem.Cost;
                    break;
                case Currency.Wins:
                    return;
            }
            currentItem.StatePPValue = 2;
            currentItem.Refresh();
            if (selectedItem != null)
            {
                selectedItem.StatePPValue = 1;
                selectedItem.Refresh();
            }
            selectedItem = currentItem;
            SelectedButton.SetActive(true);
            OnBuy?.Invoke();
        }
    }
}
