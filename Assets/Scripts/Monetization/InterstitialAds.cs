using System.Collections.Generic;
using UnityEngine;

public class InterstitialAds : MonoBehaviour
{
    private const string interstitialAdUnitId = "9454ad4f18bc9b91";
    private int retryAttempt;

    public bool IsReady => MaxSdk.IsInterstitialReady(interstitialAdUnitId);

    public void Initialize()
    {
        MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialDismissedEvent;
        MaxSdkCallbacks.OnInterstitialClickedEvent += MaxSdkCallbacks_OnInterstitialClickedEvent;
        MaxSdkCallbacks.OnInterstitialDisplayedEvent += OnInterstitialDisplayedEvent;
        isSended = false;
        LoadInterstitial();
    }

    public void Show()
    {
        MaxSdk.ShowInterstitial(interstitialAdUnitId);
    }
    
    private bool isSended;
    private void MaxSdkCallbacks_OnInterstitialClickedEvent(string adUnitId)
    {
        var metrica = AppMetrica.Instance;
        var parametrs = new Dictionary<string, object>
            {
            {"ad_type", "interstitial"},
            {"placement", "end_day_ad"},
            {"result", "clicked"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            };
        if (!isSended)
        {
            isSended = true;
            metrica.ReportEvent("video_ads_watch", parametrs);
        }
        
    }

    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(interstitialAdUnitId);
    }
    private void OnInterstitialDisplayedEvent(string adUnitId)
    {
        var metrica = AppMetrica.Instance;
        var parametrs = new Dictionary<string, object>
        {
            {"ad_type", "interstitial"},
            {"placement", "end_day_ad"},
            {"result", "watched"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        };
        if (!isSended)
        {
            isSended = true;
            metrica.ReportEvent("video_ads_watch", parametrs);
        }
    }

    private void OnInterstitialLoadedEvent(string adUnitId)
    {
        retryAttempt = 0;
    }

    private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
    {
        retryAttempt++;
        var retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttempt));
        Invoke(nameof(LoadInterstitial), retryDelay);
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId)
    {
        var metrica = AppMetrica.Instance;
        var parametrs = new Dictionary<string, object>
            {
            {"ad_type", "interstitial"},
            {"placement", "end_day_ad"},
            {"result", "canceled"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            };
        if (!isSended)
        {
            isSended = true;
            metrica.ReportEvent("video_ads_watch", parametrs);
        }
        LoadInterstitial();
    }
}
