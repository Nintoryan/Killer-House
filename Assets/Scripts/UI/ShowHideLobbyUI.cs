using UnityEngine;

public class ShowHideLobbyUI : MonoBehaviour
{
    public GameObject LobbyUI;
    private bool isHidden;
    public void ShowHide()
    {
        LobbyUI.SetActive(isHidden);
        isHidden = !isHidden;
    }
}
