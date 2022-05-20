using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isAttack", false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Player>().isAttack = false;    
    }
}
