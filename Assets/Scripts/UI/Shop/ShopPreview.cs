using System;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UserData;

namespace Shop
{
    public class ShopPreview : MonoBehaviour
    {
        [SerializeField] private GameObject[] AllSkins;
        private GameObject CurrentSkin;
        [SerializeField] private AnimatorController[] AllDances;
        [SerializeField] private Button BuyButton;
        [SerializeField] private Image BuyButtonCurrency;
        [SerializeField] private GameObject SelectedButton;
        [SerializeField] private Sprite Skull;
        [SerializeField] private Sprite Key;
        [SerializeField] private TMP_Text Price;
        private Item CurrentItem;
        public Item SelectedItem;
        public static ShopPreview Instance;

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
            CurrentSkin.GetComponent<Animator>().runtimeAnimatorController =
                AllDances[PlayerPrefs.GetInt($"Selected{Type.Dance}")];
        }

        public void Select(Item _item)
        {
            switch (_item.CurrentState)
            {
                case State.Locked:
                    SelectedButton.SetActive(false);
                    Change(_item);
                    if (_item._currency == Currency.Money)
                    {
                        if (Wallet.Balance >= _item.Cost)
                        {
                            BuyButton.gameObject.SetActive(true);
                            BuyButton.interactable = true;
                        }
                        else
                        {
                            BuyButton.gameObject.SetActive(true);
                            BuyButton.interactable = false;
                        }
                        Price.text = _item.Cost.ToString();
                        BuyButtonCurrency.sprite = Skull;
                    }
                    else
                    {
                        if (Wallet.Keys >= _item.Cost)
                        {
                            BuyButton.gameObject.SetActive(true);
                            BuyButton.interactable = true;
                        }
                        else
                        {
                            BuyButton.gameObject.SetActive(true);
                            BuyButton.interactable = false;
                        }
                        Price.text = _item.Cost.ToString();
                        BuyButtonCurrency.sprite = Key;
                    }
                    break;
                case State.Unlocked:
                    Change(_item);
                    _item.StatePPValue = 2;
                    SelectedButton.SetActive(true);
                    _item.Refresh();
                    if (SelectedItem != null)
                    {
                        SelectedItem.StatePPValue = 1;
                        SelectedItem.Refresh(); 
                    }
                    SelectedItem = _item;
                    break;
                case State.Selected:
                    SelectedButton.SetActive(true);
                    break;
            }
            if (CurrentItem != null)
            {
                CurrentItem.Deselect();
            }
            CurrentItem = _item;
        }

        private void Change(Item _item)
        {
            switch (_item._type)
            {
                case Type.Skin:
                    CurrentSkin.SetActive(false);
                    CurrentSkin = AllSkins[_item.PPID];
                    CurrentSkin.SetActive(true);
                    CurrentSkin.GetComponent<Animator>().runtimeAnimatorController =
                        AllDances[PlayerPrefs.GetInt($"Selected{Type.Dance}")];
                    break;
                case Type.Dance:
                    CurrentSkin.GetComponent<Animator>().runtimeAnimatorController = AllDances[_item.PPID];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Buy()
        {
            if (Wallet.Balance >= CurrentItem.Cost)
            {
                Wallet.Balance -= CurrentItem.Cost;
                CurrentItem.StatePPValue = 2;
                CurrentItem.Refresh();
                if (SelectedItem != null)
                {
                    SelectedItem.StatePPValue = 1;
                    SelectedItem.Refresh(); 
                }
                SelectedItem = CurrentItem;
                SelectedButton.SetActive(true);
                OnBuy?.Invoke();
            }
            else
            {
                Debug.LogError("У тебя нет денег на этот товар!");
            }
        }

    }
}
