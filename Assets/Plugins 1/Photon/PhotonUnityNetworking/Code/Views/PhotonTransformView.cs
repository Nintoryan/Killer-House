// ----------------------------------------------------------------------------
// <copyright file="PhotonTransformView.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2018 Exit Games GmbH
// </copyright>
// <summary>
//   Component to synchronize Transforms via PUN PhotonView.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------


using System;
using UnityEngine;

namespace Photon.Pun
{
    [AddComponentMenu("Photon Networking/Photon Transform View")]
    [HelpURL("https://doc.photonengine.com/en-us/pun/v2/gameplay/synchronization-and-state")]
    public class PhotonTransformView : MonoBehaviourPun, IPunObservable
    {
        private float m_Distance;
        private float m_Angle;

        private Vector3 m_Direction;
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale;

        bool m_firstTake;

        public void Awake()
        {
            m_StoredPosition = transform.localPosition;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        void OnEnable()
        {
            m_firstTake = true;
        }

        public void Update()
        {
            if (!photonView.IsMine)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, m_NetworkPosition, m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, m_NetworkRotation, m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (m_SynchronizePosition)
                {
                    m_Direction = transform.localPosition - m_StoredPosition;
                    m_StoredPosition = transform.localPosition;

                    stream.SendNext(transform.localPosition);
                    stream.SendNext(m_Direction);
                }

                if (m_SynchronizeRotation)
                {
                    stream.SendNext(transform.localRotation);
                }

                if (m_SynchronizeScale)
                {
                    stream.SendNext(transform.localScale);
                }
            }
            else
            {


                if (m_SynchronizePosition)
                {
                    try
                    {
                        m_NetworkPosition = (Vector3)stream.ReceiveNext();
                        m_Direction = (Vector3)stream.ReceiveNext();
                    }
                    catch (Exception e)
                    {
                        for (int i = 0; i < stream.ToArray().Length; i++)
                        {
                            Debug.LogError(stream.ToArray()[i]);
                        }
                        throw;
                    }
                    

                    if (m_firstTake)
                    {
                        transform.localPosition = m_NetworkPosition;
                        m_Distance = 0f;
                    }
                    else
                    {
                        float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                        m_NetworkPosition += m_Direction * lag;
                        m_Distance = Vector3.Distance(transform.localPosition, m_NetworkPosition);
                    }


                }

                if (m_SynchronizeRotation)
                {
                    m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    if (m_firstTake)
                    {
                        m_Angle = 0f;
                        transform.localRotation = m_NetworkRotation;
                    }
                    else
                    {
                        m_Angle = Quaternion.Angle(transform.localRotation, m_NetworkRotation);
                    }
                }

                if (m_SynchronizeScale)
                {
                    transform.localScale = (Vector3)stream.ReceiveNext();
                }

                if (m_firstTake)
                {
                    m_firstTake = false;
                }
            }
        }
    }
}