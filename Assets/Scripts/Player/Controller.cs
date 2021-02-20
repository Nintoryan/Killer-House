using System.Collections;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Controller : MonoBehaviour,IPunObservable
    {
        public CharacterController controller;
        [SerializeField] private FloatingJoystick _floatingJoystick;
        [SerializeField] private Camera _camera;
        public Skills _skills;
        public Transform NickNameCanvas;
        [SerializeField] private TMP_Text NickName;
        public Chat _chat;
        [SerializeField] private SphereCollider _deadBodyCollider;
        public InGameUI _InGameUI;
        public Body _Body;
        public PhotonView _photonView;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        private float playerSpeed = 3.5f;
        private float turnSmoothTime = 0.1f;
        private float turnSmoothVelocity;
        private static readonly int Status = Animator.StringToHash("status");
        private Vector3 BodyCamDistance;
        private Vector3 NickNameDistance;
        public bool isImposter;
        [Header("Sound")]
        [SerializeField] private AudioSource _RunAudioSource;

        [SerializeField] private AudioSource _WalkAudioSource;
        public AudioSource UiaAudioSource;

        [SerializeField] private AudioListener _audioListener;
        public string Name =>_photonView.Owner.NickName;

        private Transform NickNameTarget;

        private int _localNumber=-1;

        private int _skinID = -1;

        public int SkinID
        {
            get => _skinID;
            private set
            {
                if (value != _skinID)
                {
                    _skinID = value;
                    _Body.Initialize(value);
                }
            }
        }
        public int LocalNumber
        {
            get => _localNumber;
            private set => _localNumber = value;
        }
        private float gravityValue = -981f;
        public bool IsDead { get; private set;}
        public int AvaliableQuestsAmount; 

        private void Start()
        {
            GameManager.Instance.AddPlayer(this);
            NickName.text = _photonView.Owner.NickName;
            StartCoroutine(LoadLocalNumber());
            if (!_photonView.IsMine)
            {
                _camera.gameObject.SetActive(false);
                Destroy(_audioListener);
            }
            else
            {
                BodyCamDistance = _camera.transform.position - controller.transform.position;
                GameManager.Instance.LocalPlayer = this;
                _skills.SetKillingActive(false);
                _skills.SetAlarmButtonActive(false);
                _skills.SetInteractButtonActive(false);
                _chat.Initialize(_photonView.Owner.NickName,PhotonNetwork.CurrentRoom.Name);
                SkinID = PlayerPrefs.GetInt("SelectedSkin");
            }
            NickNameTarget = controller.transform;
        }

        private IEnumerator LoadLocalNumber()
        {
            yield return new WaitForSecondsRealtime(0.75f);
            if (LocalNumber == -1)
            {
                var busyNumbers = (from p in GameManager.Instance._players where p != this select p._localNumber).ToList();
                for (int i = 0; i < 10; i++)
                {
                    if (busyNumbers.Contains(i)) continue;
                    LocalNumber = i;
                    break;
                }
            }
        }

        private float directionmagnitude;
        private Vector3 direction;

        private void Update()
        {
            if (_photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.K) && !IsDead)
                {
                    SetDead();
                }
                direction = new Vector3(_floatingJoystick.Direction.x - _floatingJoystick.Direction.y, 0,
                    _floatingJoystick.Direction.x + _floatingJoystick.Direction.y);
                directionmagnitude = direction.magnitude;
                if (!IsDead)
                {
                    AliveMove(direction);
                }
                else
                {
                    DeadMove(direction);
                }

                _InGameUI.MoveMe(controller.transform.position);
            }

            if (directionmagnitude >= 0.05f && !IsDead)
            {
                
                if (directionmagnitude > 0.6f)
                {
                    //Бег
                    _RunAudioSource.UnPause();
                    _WalkAudioSource.Pause();
                }
                else
                {
                    //Шаг
                    _RunAudioSource.Pause();
                    _WalkAudioSource.UnPause();
                }
            }
            else
            {
                _RunAudioSource.Pause();
                _WalkAudioSource.Pause();
            }
            NickNameCanvas.position =  NickNameTarget.position + new Vector3(0, 1.5f, 0);
            _camera.transform.position = BodyCamDistance + controller.transform.position;
        }

        public void DisableNickName()
        {
            NickNameCanvas.gameObject.SetActive(false);
        }

        public void ActivateNickName()
        {
            NickNameCanvas.gameObject.SetActive(true);
        }

        private void DeadMove(Vector3 direction)
        {
            var NewPosition = _Body.transform.position;
            NewPosition += direction * (playerSpeed * Time.deltaTime);
            NewPosition = new Vector3(
                Mathf.Clamp(NewPosition.x,-15,90),
                NewPosition.y,
                Mathf.Clamp(NewPosition.z,-25,115)
            );
            _Body.transform.position = NewPosition;
            if (direction.magnitude >= 0.05f)
            {
                _skills.isDancing = false;
                _Body._Animator.SetInteger(Status, direction.magnitude > 0.6f ? 2 : 1);
                var targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                    turnSmoothTime);
                _Body.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else
            {
                if (!_skills.isDancing)
                {
                    _Body._Animator.SetInteger(Status, 0);
                }
            }
            if (direction != Vector3.zero)
            {
                _Body.transform.forward = direction;
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
                _skills.isDancing = false;
                _Body._Animator.SetInteger(Status, direction.magnitude > 0.6f ? 2 : 1);
                var targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                    turnSmoothTime);
                controller.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                controller.Move(direction * (Time.deltaTime * playerSpeed));
            }
            else
            {
                if (!_skills.isDancing)
                {
                    _Body._Animator.SetInteger(Status, 0);
                }
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
                _Body._Animator.SetInteger(Status,-1);
            else
            {
                SetNickNameTarget(_Body.AliveGraphicsTransform());
            }
            _skills.SetKillingActive(false);
            _skills.SetAlarmButtonActive(false);
            controller.enabled = false;
            playerSpeed = 5f;
            _deadBodyCollider.transform.SetParent(null);
            _deadBodyCollider.gameObject.SetActive(true);
            _Body.SwitchToDead();
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
            _deadBodyCollider.gameObject.SetActive(false);
            _Body.Hide();
        }

        public void DisableControll()
        {
            _floatingJoystick.enabled = false;
            direction = Vector3.zero;
        }

        public void ActivateControll()
        {
            _floatingJoystick.enabled = true;
        }

        public void SetCamFOV(int fovValue)
        {
            _camera.fieldOfView = fovValue;
        }

        public void SetNickNameTarget(Transform newTarget)
        {
            NickNameTarget = newTarget;
        }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (!GameManager.Instance.isGameStarted)
            {
                if (stream.IsWriting)
                {
                    stream.SendNext(LocalNumber);
                    stream.SendNext(SkinID);
                }
                else if (stream.IsReading)
                {
                    LocalNumber = (int) stream.ReceiveNext();
                    SkinID = (int) stream.ReceiveNext();
                }
            }
        }
    }
}
