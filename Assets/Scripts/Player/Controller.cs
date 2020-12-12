using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = System.Collections.Hashtable;

#pragma warning disable CS0649
namespace AAPlayer
{
    public class Controller : MonoBehaviour, IPunObservable
    {
        public CharacterController controller;
        [SerializeField] private FloatingJoystick _floatingJoystick;
        [SerializeField] private Camera _camera;
        public Skills _skills;
        [SerializeField] private Transform NickNameCanvas;
        [SerializeField] private TMP_Text NickName;
        [SerializeField] private Chat _chat;
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
        public bool isInShortCut;
        [Header("Sound")]
        [SerializeField] private AudioSource _RunAudioSource;

        [SerializeField] private AudioSource _WalkAudioSource;
        public AudioSource UiaAudioSource;

        [SerializeField] private AudioListener _audioListener;
        public string Name =>_photonView.Owner.NickName;

        private int _localNumber=-1;
        public int LocalNumber
        {
            get => _localNumber;
            set
            {
                if (_localNumber != value)
                {
                    _localNumber = value;
                    _Body.Initialize(value);
                }
            }
        }
        private float gravityValue = -981f;
        public bool IsDead { get; private set;}
        public int AvaliableQuestsAmount; 

        private void Start()
        {
            GameManager.Instance.AddPlayer(this);
            NickName.text = _photonView.Owner.NickName;
            if (!_photonView.IsMine)
            {
                _camera.gameObject.SetActive(false);
                LocalNumber = _photonView.Owner.ActorNumber-1;
                Destroy(_audioListener);
            }
            else
            {
                BodyCamDistance = _camera.transform.position - controller.transform.position;
                GameManager.Instance.LocalPlayer = this;
                LocalNumber = PhotonNetwork.LocalPlayer.ActorNumber-1;
                _skills.SetKillingActive(false);
                _skills.SetAlarmButtonActive(false);
                _skills.SetInteractButtonActive(false);
                _chat.Initialize(_photonView.Owner.NickName,PhotonNetwork.CurrentRoom.Name);
            }
        }

        private float directionmagnitude;
        private Vector3 direction;

        private void Update()
        {
            if (_photonView.IsMine)
            {
                direction = new Vector3(_floatingJoystick.Direction.x - _floatingJoystick.Direction.y, 0,
                    _floatingJoystick.Direction.x + _floatingJoystick.Direction.y);
                directionmagnitude = direction.magnitude;
                if (!IsDead)
                {
                    AliveMove(direction);
                }
                else
                {
                    _camera.transform.position += direction * (playerSpeed * 2 * Time.deltaTime);
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

            NickNameCanvas.position = controller.transform.position + new Vector3(0, 1.5f, 0);
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
                _Body._Animator.SetInteger(Status, direction.magnitude > 0.6f ? 2 : 1);
                var targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                    turnSmoothTime);
                controller.transform.rotation = Quaternion.Euler(0f, angle, 0f);
                controller.Move(direction * (Time.deltaTime * playerSpeed));
                _camera.transform.position = BodyCamDistance + controller.transform.position;
            }
            else
            {
                _Body._Animator.SetInteger(Status, 0);
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
            DisableNickName();
            _skills.SetKillingActive(false);
            _skills.SetAlarmButtonActive(false);
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

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(directionmagnitude);
                stream.SendNext(isInShortCut);
            }else if (stream.IsReading)
            {
                directionmagnitude = (float)stream.ReceiveNext();
                isInShortCut = (bool) stream.ReceiveNext();
            }
        }
        
    }
}
