using System;
using System.Collections.Generic;
using UnityEngine;

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
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration => {
                _interstitialAd.Initialize();
                RewardedAd.Initialize();
            };
            MaxSdk.SetSdkKey("6AQkyPv9b4u7yTtMH9PT40gXg00uJOTsmBOf7hDxa_-FnNZvt_qTLnJAiKeb5-2_T8GsI_dGQKKKrtwZTlCzAR");
            MaxSdk.InitializeSdk();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public void ShowRewarded(string placement)
    {
        RewardedAd.Show(placement);
    }

    public void ShowInterstitial()
    {
        var metrica = AppMetrica.Instance;
        var parametrs = new Dictionary<string, object>
        {
            {"ad_type", "interstitial"},
            {"placement", "match"},
            {"result", IsInterstitialReady?"success":"not_available"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        };
        metrica.ReportEvent("video_ads_available", parametrs);
        var parametrs1 = new Dictionary<string, object>
        {
            {"ad_type", "interstitial"},
            {"placement", "match"},
            {"result", "start"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        };
        if (IsInterstitialReady)
        {
            metrica.ReportEvent("video_ads_started", parametrs1);
        }
        _interstitialAd.Show();
    }
}
