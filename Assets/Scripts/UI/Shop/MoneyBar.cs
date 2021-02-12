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
        _shopPreview.OnBuy += Refresh;
    }

    private void Refresh()
    {
        Money.text = Wallet.Balance.ToString();
        Wins.text = Statistics.Wins.ToString();
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
        }
    }
}
