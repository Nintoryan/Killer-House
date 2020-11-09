using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using UnityEngine;

#pragma warning disable CS0649  
public class PlayerController : MonoBehaviour
{
    [SerializeField]private Animator _animator;
    [SerializeField]private PhotonView _photonView;
    [SerializeField]private CharacterController controller;
    [SerializeField]private FloatingJoystick _floatingJoystick;
    [SerializeField]private Camera _camera;
    [SerializeField]private SkinnedMeshRenderer _mesh;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 3.5f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private static readonly int Status = Animator.StringToHash("status");
    private Vector3 BodyCamDistance;
    private string Name;
    private int ID;
    private MaterialPropertyBlock _propBlock;
    private void Start()
    {
        if (!_photonView.IsMine)
        {
            _camera.gameObject.SetActive(false);
        }
        else
        {
            _propBlock = new MaterialPropertyBlock();
            BodyCamDistance = _camera.transform.position - controller.transform.position;
            Initialize(PhotonNetwork.LocalPlayer.ActorNumber,PhotonNetwork.NickName);
        }
    }

    private void Initialize(int id, string ActorName)
    {
        ID = id;
        Name = ActorName;
        _mesh.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", LobbyManager.GetColor(id-1));
        _mesh.SetPropertyBlock(_propBlock);
    }

    public void UpdateColor(int id)
    {
        _propBlock = new MaterialPropertyBlock(); 
        _mesh.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", LobbyManager.GetColor(id-1));
        _mesh.SetPropertyBlock(_propBlock);
    }

    private void Update()
    {
        if (!_photonView.IsMine) return;
        
        var direction = new Vector3(_floatingJoystick.Direction.x, 0, _floatingJoystick.Direction.y);
        if (direction.magnitude >= 0.05f)
        {
            _animator.SetInteger(Status,direction.magnitude > 0.6f ? 2 : 1);
            var targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y,targetAngle,ref turnSmoothVelocity,turnSmoothTime);
            controller.transform.rotation = Quaternion.Euler(0f,angle,0f);
            controller.Move(direction * (Time.deltaTime * playerSpeed));
            _camera.transform.position = BodyCamDistance + controller.transform.position;
        }
        else
        {
            _animator.SetInteger(Status,0);
        }
        if (direction != Vector3.zero)
        {
            controller.transform.forward = direction;
        }
    }
}
