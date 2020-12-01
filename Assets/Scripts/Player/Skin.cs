using Photon.Pun;
using UnityEngine;
#pragma warning disable CS0649
public class Skin : MonoBehaviour,IPunObservable
{
    
    [SerializeField]private SkinnedMeshRenderer _mesh;

    public int ColorID
    {
        get => _colorID;
        set
        {
            if (_colorID != value)
            {
                UpdateColor(value);
            }
            _colorID = value;
            
        }
    }

    public Color _Color => LobbyManager.GetColor(_colorID - 1);
    private MaterialPropertyBlock _propBlock;
    private int _colorID;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ColorID);
        }
        else
        {
            ColorID = (int) stream.ReceiveNext();
            
        }
    }
    public void UpdateColor(int id)
    {
        _propBlock = new MaterialPropertyBlock(); 
        _mesh.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", LobbyManager.GetColor(id-1));
        _mesh.SetPropertyBlock(_propBlock);
    }
}
