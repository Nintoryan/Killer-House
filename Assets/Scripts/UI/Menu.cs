using System;
using UnityEngine;
using DG.Tweening;


public class Menu : MonoBehaviour
{
    [SerializeField] private RectTransform ShopPanel;
    [SerializeField] private RectTransform MainPanel;

    public void OpenShopPanel()
    {
        ShopPanel.DOAnchorPosX(0, 0.5f);
        MainPanel.DOAnchorPosX(-3000, 0.5f);
    }

    public void CloseShopPanel()
    {
        ShopPanel.DOAnchorPosX(3000, 0.5f);
        MainPanel.DOAnchorPosX(0, 0.5f);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}
