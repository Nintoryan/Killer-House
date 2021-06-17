﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InterstitialAds : MonoBehaviour
{
    private const string interstitialAdUnitId = "9454ad4f18bc9b91";
    private int retryAttempt;
    public event UnityAction OnDone;

    public bool IsReady => MaxSdk.IsInterstitialReady(interstitialAdUnitId);

    public void Initialize()
    {
        MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialDismissedEvent;
        MaxSdkCallbacks.OnInterstitialClickedEvent += MaxSdkCallbacks_OnInterstitialClickedEvent;
        MaxSdkCallbacks.OnInterstitialDisplayedEvent += OnInterstitialDisplayedEvent;
        LoadInterstitial();
    }

    public void Show()
    {
        isSended = false;
        MaxSdk.ShowInterstitial(interstitialAdUnitId);
    }
    
    private bool isSended;
    private void MaxSdkCallbacks_OnInterstitialClickedEvent(string adUnitId)
    {
        var metrica = AppMetrica.Instance;
        var parametrs = new Dictionary<string, object>
            {
            {"ad_type", "interstitial"},
            {"placement", "match"},
            {"result", "clicked"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            };
        if (!isSended)
        {
            isSended = true;
            metrica.ReportEvent("video_ads_watch", parametrs);
            metrica.SendEventsBuffer();
            OnDone?.Invoke();
            OnDone = null;
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
            {"placement", "match"},
            {"result", "watched"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
        };
        if (!isSended)
        {
            isSended = true;
            metrica.ReportEvent("video_ads_watch", parametrs);
            metrica.SendEventsBuffer();
            OnDone?.Invoke();
            OnDone = null;
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
            {"placement", "match"},
            {"result", "canceled"},
            {"connection", Application.internetReachability != NetworkReachability.NotReachable}
            };
        if (!isSended)
        {
            isSended = true;
            metrica.ReportEvent("video_ads_watch", parametrs);
            metrica.SendEventsBuffer();
            OnDone?.Invoke();
            OnDone = null;
        }
        LoadInterstitial();
    }
}
