using UnityEngine;

public class MinigamePresenter : MonoBehaviour
{
    public Animator _Animator;
    private static readonly int Status = Animator.StringToHash("status");

    public void Play()
    {
        _Animator.SetInteger(Status,1);
    }

    public void Stop()
    {
        _Animator.SetInteger(Status,0);
    }
}
