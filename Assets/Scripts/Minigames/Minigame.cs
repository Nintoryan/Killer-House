using UnityEngine;

public class Minigame : MonoBehaviour
{
    public int Number;
    protected void Complete()
    {
        GameManager.Instance.AllMinigames[Number].isComplete = true;
        GameManager.Instance.LocalPlayer._skills._interactButton.interactable = false;
    }
}
