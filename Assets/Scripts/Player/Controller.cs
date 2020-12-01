using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterController controller;
        [SerializeField] private FloatingJoystick _floatingJoystick;
        [SerializeField] private Camera _camera;
        public Skills _skills;
        [SerializeField] private Transform NickNameCanvas;
        [SerializeField] private TMP_Text NickName;
        [SerializeField] private Chat _chat;
        [SerializeField] private SphereCollider _deadBodyCollider;

        public Body _Body;
        public Skin _skin;
        public PhotonView _photonView;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        private float playerSpeed = 3.5f;
        private float turnSmoothTime = 0.1f;
        private float turnSmoothVelocity;
        private static readonly int Status = Animator.StringToHash("status");
        private Vector3 BodyCamDistance;
        private Vector3 NickNameDistance;
        public string Name =>_photonView.Owner.NickName;
        public int ID;
        private float gravityValue = -981f;
        public bool IsDead { get; private set;}

        private void Start()
        {
            GameManager.Instance.AddPlayer(this);
            NickName.text = _photonView.Owner.NickName;
            if (!_photonView.IsMine)
            {
                _camera.gameObject.SetActive(false);
            }
            else
            {
                BodyCamDistance = _camera.transform.position - controller.transform.position;
                NickNameDistance = NickNameCanvas.position - controller.transform.position;
                _skin.ColorID = PhotonNetwork.LocalPlayer.ActorNumber;
                ID = PhotonNetwork.LocalPlayer.ActorNumber;
                GameManager.Instance.LocalPlayer = this;
                _skills.DiableKilling();
                _chat.Initialize(_photonView.Owner.NickName,PhotonNetwork.CurrentRoom.Name);
            }
        }

        private void Update()
        {
            if (_photonView.IsMine)
            {
                var direction = new Vector3(_floatingJoystick.Direction.x - _floatingJoystick.Direction.y, 0,
                    _floatingJoystick.Direction.x + _floatingJoystick.Direction.y);
                if (!IsDead)
                {
                    AliveMove(direction);
                }
                else
                {
                    _camera.transform.position += direction * (playerSpeed * 2 * Time.deltaTime);
                }
            }
            else
            {
                
            }
            NickNameCanvas.position = NickNameDistance + controller.transform.position;
        }

        public void UpdateCameraPos()
        {
            if (_photonView.IsMine)
            {
                if (!IsDead)
                {
                    _camera.transform.position = BodyCamDistance + controller.transform.position;
                }
                else
                {
                    _camera.transform.position = new Vector3(11,20,-13);
                }
            }
        }
        
        public void DisableNickName()
        {
            NickNameCanvas.gameObject.SetActive(false);
        }

        public void ActivateNickName()
        {
            NickNameCanvas.gameObject.SetActive(true);
        }

        private void AliveMove(Vector3 direction)
        {
            groundedPlayer = controller.isGrounded;
            if (groundedPlayer && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
            if (direction.magnitude >= 0.05f)
            {
                _animator.SetInteger(Status, direction.magnitude > 0.6f ? 2 : 1);
                var targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                    turnSmoothTime);
                controller.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                controller.Move(direction * (Time.deltaTime * playerSpeed));
                _camera.transform.position = BodyCamDistance + controller.transform.position;
            }
            else
            {
                _animator.SetInteger(Status, 0);
            }

            if (direction != Vector3.zero)
            {
                controller.transform.forward = direction;
            }
        }
        

        public void DieEvent()
        {
            int sendingData = _photonView.Owner.ActorNumber;
            RaiseEventOptions options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(42, sendingData, options, sendOptions);
            Debug.Log($"Я убил игрока {sendingData}");
        }

        public void SetDead()
        {
            IsDead = true;
            if(_photonView.IsMine)
                _animator.SetInteger(Status,-1);
            DisableNickName();
            _skills.DiableKilling();
            controller.enabled = false;
            _deadBodyCollider.gameObject.SetActive(true);
            Debug.Log($"Убили игрока {_photonView.Owner.ActorNumber}");
        }

        public void DisableDeadBodyEvent()
        {
            int sendingData = _photonView.Owner.ActorNumber;
            RaiseEventOptions options = new RaiseEventOptions {Receivers = ReceiverGroup.All};
            SendOptions sendOptions = new SendOptions {Reliability = true};
            PhotonNetwork.RaiseEvent(50, sendingData, options, sendOptions);
        }
        public void DisableDeadBody()
        {
            _Body.HideDeadBody();
            _deadBodyCollider.gameObject.SetActive(false);
        }

        public void DisableControll()
        {
            _floatingJoystick.enabled = false;
        }

        public void ActivateControll()
        {
            _floatingJoystick.enabled = true;
        }
    }
}
