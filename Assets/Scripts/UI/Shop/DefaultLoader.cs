using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class DefaultLoader : MonoBehaviour
{
    private void Awake()
    {
        if (!PlayerPrefs.HasKey("Default"))
        {
            SetDefaultValues();
        }
    }
    private void SetDefaultValues()
    {
        PlayerPrefs.SetInt("Default",1);
        for (int i = 0; i < 8; i++)
        {
            PlayerPrefs.SetInt($"Skin{i}",1);
        }
        var RandomInt = Random.Range(0, 8);
        PlayerPrefs.SetInt($"Skin{RandomInt}",2);
        PlayerPrefs.SetInt("SelectedSkin",RandomInt);
        var hashtable = new Hashtable {["SelectedSkin"] = RandomInt};
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        PlayerPrefs.SetInt("Dance0",2);
        PlayerPrefs.SetInt("SelectedDance",0);
    }
}
