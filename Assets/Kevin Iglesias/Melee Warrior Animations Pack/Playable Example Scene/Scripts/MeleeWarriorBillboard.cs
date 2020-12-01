using UnityEngine;
using System.Collections;

namespace KevinIglesias {
    public class MeleeWarriorBillboard : MonoBehaviour
    {
        private Camera cam;
     
        void Start()
        {
            cam = Camera.main;
        }
     
        //Orient the camera in LateUpdate to avoid jitter movement
        void LateUpdate()
        {
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up);
        }
    }
}
