using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias {
    
    public class MeleeWarriorAttackScript : StateMachineBehaviour {
        
        MeleeWarriorUnitScript mWUS;
        Rigidbody rb;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        
            if(mWUS == null)
            {
                mWUS = animator.GetComponent<MeleeWarriorUnitScript>();
            }
            
            if(rb == null)
            {
                rb = animator.GetComponent<Rigidbody>();
            }
            
            //Attack
            if(mWUS != null)
            {
                mWUS.Attack();
            }
            
            //Avoid being pushed when attacking
            if(rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            if(rb != null)
            {
                //Enable push again
                if(!animator.GetBool("Attack"))
                {
                    rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                }
            }
        }
    }
}