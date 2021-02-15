using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class NoAdsPurchase : MonoBehaviour
{
    private Button _button;

    private void Start()
    {
        _button = GetComponent<Button>();
        if (PlayerPrefs.GetInt("NoAds") == 1)
        {
            gameObject.SetActive(false);
        }
    }
    

    public void OnPurchaseSuccess(Product _product)
    {
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
        catch { }

        try
        {
            metrica.ReportEvent("payment_succeed",parametrs);
        }
        catch
        {
        }
        
        //AppMetrica
        
        Debug.Log("Успешно куплено");
    }
}
