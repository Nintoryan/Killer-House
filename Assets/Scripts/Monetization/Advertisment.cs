﻿using UnityEngine;

public class Advertisment : MonoBehaviour
{
    [SerializeField] private InterstitialAds _interstitialAd;
    public RewardedAds RewardedAd;

    public bool IsInterstitialReady => _interstitialAd.IsReady;
    public bool IsRewardedReady => RewardedAd.IsReady;

    public static Advertisment Instance
    {
        get;
        private set;
    }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration => {
                _interstitialAd.Initialize();
                RewardedAd.Initialize();
            };
            MaxSdk.SetSdkKey("6AQkyPv9b4u7yTtMH9PT40gXg00uJOTsmBOf7hDxa_-FnNZvt_qTLnJAiKeb5-2_T8GsI_dGQKKKrtwZTlCzAR");
            MaxSdk.InitializeSdk();
        }
        else
        {
            if (Instance != this)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void ShowRewarded()
    {
        RewardedAd.Show();
    }

    public void ShowInterstitial()
    {
        _interstitialAd.Show();
    }
}
