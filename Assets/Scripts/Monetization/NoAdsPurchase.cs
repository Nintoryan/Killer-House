using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;
using UnityEngine.Purchasing;

public class NoAdsPurchase : MonoBehaviour
{
    private static bool buttonWasClicked;
    private void Awake()
    {
        if (PlayerPrefs.GetInt("NoAds") == 1)
        {
            Destroy(gameObject);
            //
        }
    }

    public void SetButtonWasClicked(bool isTrue)
    {
        buttonWasClicked = isTrue;
        Debug.Log($"buttonWasClicked:{buttonWasClicked}");
    }

    public void Buy(Product _product)
    {
        if (PlayerPrefs.GetInt("NoAds") == 1 || !buttonWasClicked)
        {
            PlayerPrefs.SetInt("NoAds",1);
            Destroy(gameObject);
            return;
        }
        PlayerPrefs.SetInt("NoAds",1);
        //AppMetrica
        var metrica = AppMetrica.Instance;
        var parametrs = new Dictionary<string, object>
        {
            {"inapp_id","no_ads"},
            {"currency",_product.metadata.isoCurrencyCode},
            {"price", (double)_product.metadata.localizedPrice},
            {"inapp_type","noads"}
        };
        var revenue = new YandexAppMetricaRevenue(_product.metadata.localizedPrice,
            _product.metadata.isoCurrencyCode);
        var yandexAppMetricaAndroid = new YandexAppMetricaAndroid();
        //AppsFlyer//
        var purchaseEvent = new Dictionary<string, string>
        {
            {"af_revenue", _product.metadata.localizedPriceString},
            {"af_currency", _product.metadata.isoCurrencyCode},
            {"af_quantity", "1"},
            {"af_content_id", "001"},
            {"order_id", "9277"},
            {"af_receipt_id", "9277"}
        };
        AppsFlyer.sendEvent("af_purchase", purchaseEvent);
        /////
        try
        {
            yandexAppMetricaAndroid.ReportRevenue(revenue);

        }
        catch(Exception e)
        {
            Debug.Log(e);
        }

        try
        {
            metrica.ReportEvent("payment_succeed",parametrs);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        
        //AppMetrica
        gameObject.SetActive(false);
        Debug.Log("Успешно куплено");
    }
}
