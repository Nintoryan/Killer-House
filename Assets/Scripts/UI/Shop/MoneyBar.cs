using Shop;
using TMPro;
using UnityEngine;
using UserData;

public class MoneyBar : MonoBehaviour
{
    [SerializeField] private TMP_Text Money;
    [SerializeField] private TMP_Text Wins;
    [SerializeField] private ShopPreview _shopPreview;
    
    private void Start()
    {
        Refresh();
        if (_shopPreview != null)
        {
            _shopPreview.OnBuy += Refresh;
        }
    }

    public void Refresh()
    {
        Money.text = Wallet.Balance.ToString();
        Wins.text = Wallet.Keys.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Wallet.Balance += 100;
            Refresh();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Wallet.Keys += 1;
            Refresh();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
