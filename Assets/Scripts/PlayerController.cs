using Photon.Pun;
using UnityEngine;

#pragma warning disable CS0649  
public class PlayerController : MonoBehaviour
{
    [SerializeField]private Animator _animator;
    [SerializeField]private PhotonView _photonView;
    [SerializeField]private CharacterController controller;
    [SerializeField]private FloatingJoystick _floatingJoystick;
    [SerializeField] private Camera _camera;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 3.5f;
    private float gravityValue = -9.81f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private static readonly int Status = Animator.StringToHash("status");
    private Vector3 BodyCamDistance;
    private void Start()
    {
        if (!_photonView.IsMine)
        {
            _camera.gameObject.SetActive(false);
        }
        else
        {
            BodyCamDistance = _camera.transform.position - controller.transform.position;
        }
    }

    private void Update()
    {
        if (!_photonView.IsMine) return;
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
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
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
