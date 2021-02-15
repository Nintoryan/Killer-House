using UnityEngine;

public class GDPR : MonoBehaviour
{
    private void Start()
    {
        if (PlayerPrefs.GetInt("GDPRAccept") == 1)
        {
            gameObject.SetActive(false);
        }
    }
    public void OpenLink(string link)
    {
        Application.OpenURL(link);
    }

    public void Accept()
    {
        PlayerPrefs.SetInt("GDPRAccept",1);
    }
}
