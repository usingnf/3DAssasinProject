using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetComponent<Player>().jumpPower = 5.0f;
    }
}
