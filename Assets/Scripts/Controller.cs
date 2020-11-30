using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using DG.Tweening;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Controller : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private Animator _animator;
        public PhotonView _photonView;
        [SerializeField] private CharacterController controller;
        [SerializeField] private FloatingJoystick _floatingJoystick;
        [SerializeField] private Camera _camera;
        public Body _Body;
        public static List<Controller> AllPlayers = new List<Controller>();
        public Skin _skin;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        private float playerSpeed = 3.5f;
        private float turnSmoothTime = 0.1f;
        private float turnSmoothVelocity;
        private static readonly int Status = Animator.StringToHash("status");
        private Vector3 BodyCamDistance;
        private string Name;
        private string ID;
        private float gravityValue = -9.81f;

        private bool _isDead;
        private bool IsDead
        {
            get => _isDead;
            set
            {
                _isDead = value;
                _animator.enabled = !_isDead;
            } }


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(IsDead);   
            }
            else
            {
                IsDead = (bool)stream.ReceiveNext();
            }
        }

        private void Start()
        {
            if (!AllPlayers.Contains(this))
            {
                AllPlayers.Add(this);
                Debug.Log($"Кол-во игроков:{AllPlayers.Count}");
            }

            if (!_photonView.IsMine)
            {
                _camera.gameObject.SetActive(false);
            }
            else
            {
                BodyCamDistance = _camera.transform.position - controller.transform.position;
                Initialize(PhotonNetwork.LocalPlayer.UserId, PhotonNetwork.NickName);
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            foreach (var player in AllPlayers.Where(player => player.ID == otherPlayer.UserId))
            {
                AllPlayers.Remove(player);
                Debug.Log($"Кол-во игроков:{AllPlayers.Count}");
                Destroy(player.gameObject);
                break;
            }
        }

        private void Initialize(string id, string ActorName)
        {
            ID = id;
            Name = ActorName;
            _skin.ColorID = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        private void Update()
        {
            if (!_photonView.IsMine) return;
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
        public static void Die(Controller c)
        {
            c.IsDead = true;
            c._animator.enabled = false;
            c._floatingJoystick.enabled = false;
            var s = DOTween.Sequence();
            s.AppendInterval(1f);
            s.AppendCallback(() =>
            {
                c._camera.transform.localPosition += new Vector3(1, 1, -1) * 10;
                var RagDollParts = c.GetComponentsInChildren<Rigidbody>();
                foreach (var RagDollsPart in RagDollParts)
                {
                    RagDollsPart.isKinematic = false;
                }
                c._floatingJoystick.enabled = true;
            });
        }
    }
}
