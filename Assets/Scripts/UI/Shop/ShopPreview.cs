using System;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
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
        [SerializeField] private TMP_Text Price;
        private Item CurrentItem;
        public static ShopPreview Instance;

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
            CurrentItem = _item;
            switch (_item.CurrentState)
            {
                case State.Locked:
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
                    }
                    break;
                case State.Unlocked:
                    Change(_item);
                    _item.StatePPValue = 2;
                    BuyButton.gameObject.SetActive(false);
                    break;
                case State.Selected:
                    BuyButton.gameObject.SetActive(false);
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
                    break;
                case Type.Dance:
                    CurrentSkin.GetComponent<Animator>().runtimeAnimatorController = AllDances[_item.PPID];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
