using UnityEngine;
using UnityEngine.UI;
using UserData;

public class RewardedAdButton : MonoBehaviour
{
    private Button _button;
    [SerializeField] private MoneyBar _moneyBar;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(Watch);
        Advertisment.Instance.RewardedAd.OnReciveReward += () =>
        {
            Wallet.Balance += 150;
            _moneyBar.Refresh();
        };
    }

    private void Update()
    {
        _button.interactable = Advertisment.Instance.IsRewardedReady;
    }

    private void Watch()
    {
        Advertisment.Instance.ShowRewarded();
    }
}
