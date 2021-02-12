using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabsController : MonoBehaviour
{
    [SerializeField] private Color ActiveColor;
    [SerializeField] private Color DisableColor;
    [SerializeField] private Tab[] Tabs;

    public void SelectTab(int id)
    {
        foreach (var _tab in Tabs)
        {
            _tab.Deselect(DisableColor);
        }
        Tabs[id].Select(ActiveColor);
    }
}
[Serializable]
public class Tab
{
    public Image _Image;
    public TMP_Text _Text;
    public Sprite Acitavated;
    public Sprite Disabled;
    public GameObject Container;

    public void Select(Color a)
    {
        _Image.sprite = Acitavated;
        Container.SetActive(true);
        _Text.color = a;
    }

    public void Deselect(Color a)
    {
        _Image.sprite = Disabled;
        Container.SetActive(false);
        _Text.color = a; 
    }
}


