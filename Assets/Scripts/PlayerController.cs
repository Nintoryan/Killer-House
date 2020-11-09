using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PhotonView _photonView;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 2.0f;
    private float gravityValue = -9.81f;
    private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private FloatingJoystick _floatingJoystick;

    private void Start()
    {
        _photonView = GetComponentInChildren<PhotonView>();
        controller = GetComponentInChildren<CharacterController>();
        _floatingJoystick = GetComponentInChildren<FloatingJoystick>();
    }
    
    private void Update()
    {
        if(!_photonView.IsMine) return;
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        var direction = new Vector3(_floatingJoystick.Direction.x, 0, _floatingJoystick.Direction.y);
        if (direction.magnitude >= 0.05f)
        {
            var targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y,targetAngle,ref turnSmoothVelocity,turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f,angle,0f);
            controller.Move(direction * (Time.deltaTime * playerSpeed));
        }
        

        if (direction != Vector3.zero)
        {
            gameObject.transform.forward = direction;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}
