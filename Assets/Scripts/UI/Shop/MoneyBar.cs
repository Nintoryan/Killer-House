using Shop;
using TMPro;
using UnityEngine;
using UserData;

public class MoneyBar : MonoBehaviour
{
    [SerializeField] private TMP_Text Money;
    [SerializeField] private TMP_Text Wins;
    [SerializeField] private ShopPreview _shopPreview;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Skin0"))
        {
            GetDefaultValues();
        }
    }

    private void GetDefaultValues()
    {
        for (int i = 0; i < 8; i++)
        {
            PlayerPrefs.SetInt($"Skin{i}",1);
        }

        var RandomInt = UnityEngine.Random.Range(0, 8);
        PlayerPrefs.SetInt($"Skin{RandomInt}",2);
        PlayerPrefs.SetInt("SelectedSkin",RandomInt);
        PlayerPrefs.SetInt("Dance0",2);
        PlayerPrefs.SetInt("SelectedDance",0);
    }

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
            PlayerPrefs.DeleteAll();
        }
    }
}
