using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PairItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;
    [SerializeField] private FindThePair _myBrain;
    public int ID { get; private set; }

    public void Initialize(Sprite _sprite, int id, FindThePair findThePair)
    {
        _image.sprite = _sprite;
        _image.color = new Color(1,1,1,0);
        ID = id;
        _myBrain = findThePair;
        _button.interactable = true;
    }

    public void Touch()
    {
        _myBrain.Touch(this);
    }

    public void Open()
    {
        var s = DOTween.Sequence();
        s.AppendCallback(() => { _button.interactable = false;});
        s.Append(transform.DORotate(new Vector3(0, 90, 0), 0.15f));
        s.AppendCallback(()=>_image.color = Color.white);
        s.Append(transform.DORotate(new Vector3(0, 0, 0), 0.15f));
        s.AppendCallback(() => { _button.interactable = !locked;});
    }

    public void Close()
    {
        var s = DOTween.Sequence();
        s.AppendCallback(() => { _button.interactable = false;});
        s.Append(transform.DORotate(new Vector3(0, 90, 0), 0.15f));
        s.AppendCallback(()=>_image.color = Color.clear);
        s.Append(transform.DORotate(new Vector3(0, 0, 0), 0.15f));
        s.AppendCallback(() => { _button.interactable = !locked;});
    }

    private bool locked;
    public void LockState()
    {
        _button.interactable = false;
        locked = true;
    }
}
