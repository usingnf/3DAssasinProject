using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStandDown : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<Player>().stand = 1;
    }
}
