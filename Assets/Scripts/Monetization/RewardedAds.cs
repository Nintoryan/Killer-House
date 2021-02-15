using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RewardedAds : MonoBehaviour
{
    private const string rewardedAdUnitId = "78f4cc81fcd72a8a";
    private int retryAttempt;
    private bool isClicked = true;
    private bool isWatched = true;
    private bool isCanceled = true;

    private Dictionary<string, object> AdsAvailable;
    private Dictionary<string, object> AdsStarted;
    private Dictionary<string, object> AdsClicked;
    private Dictionary<string, object> AdsCanceled;
    private Dictionary<string, object> AdsWatched;

    private bool TriggeredOnce { get; set; } = true;
    public bool IsReady => MaxSdk.IsRewardedAdReady(rewardedAdUnitId);
    public event UnityAction OnReciveReward;
    public event UnityAction OnDismissed;

    public void SetAnalyticsData(Dictionary<string, object> adsAvailable, Dictionary<string, object> adsStarted, Dictionary<string, object> adsClicked, Dictionary<string, object> adsCanceled, Dictionary<string, object> adsWatched)
    {
        AdsAvailable = adsAvailable;
        AdsStarted = adsStarted;
        AdsClicked = adsClicked;
        AdsCanceled = adsCanceled;
        AdsWatched = adsWatched;
    }

    public void Show()
    {
        var metrica = AppMetrica.Instance;        
        metrica.ReportEvent("video_ads_available", AdsAvailable);

        isClicked = false;
        isWatched = false;
        isCanceled = false;
        TriggeredOnce = false;
        MaxSdk.ShowRewardedAd(rewardedAdUnitId);
    }
    
    public void Initialize()
    {
        MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(rewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId)
    {
        retryAttempt = 0;
    }

    private void OnRewardedAdFailedEvent(string adUnitId, int errorCode)
    {
        retryAttempt++;
        double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttempt));
        Invoke(nameof(LoadRewardedAd), (float)retryDelay);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        ClearEvents();
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId)
    {
        var metrica = AppMetrica.Instance;
        metrica.ReportEvent("video_ads_started", AdsStarted);
    }

    private void OnRewardedAdClickedEvent(string adUnitId) 
    {
        var metrica = AppMetrica.Instance;
        if (!isWatched && !isClicked)
        {
            metrica.ReportEvent("video_ads_watch", AdsClicked);
        }
        isClicked = true;
    }

    private void OnRewardedAdDismissedEvent(string adUnitId)
    {
        if (!TriggeredOnce)
        {
            OnDismissed?.Invoke();
            ClearEvents();

            // Rewarded ad is hidden.Pre - load the next ad
            var metrica = AppMetrica.Instance;
            if (!isWatched && !isCanceled)
            {
                metrica.ReportEvent("video_ads_watch", AdsCanceled);
            }
            isCanceled = true;
        }

        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward = new MaxSdk.Reward())
    {
        if (!TriggeredOnce)
        {
            // Rewarded ad was displayed and user should receive the reward
            OnReciveReward?.Invoke();
            ClearEvents();

            var metrica = AppMetrica.Instance;
            
            if (!isClicked)
            {
                metrica.ReportEvent("video_ads_watch", AdsWatched);
            }
            isWatched = true;
            TriggeredOnce = true;
        }
        
    }

    private void ClearEvents()
    {
        OnReciveReward = null;
        OnDismissed = null;
    }
}
