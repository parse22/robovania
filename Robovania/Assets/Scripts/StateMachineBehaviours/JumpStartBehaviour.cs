using UnityEngine;

public class JumpStartBehaviour : StateMachineBehaviour
{
    private RoboPawn _pawn;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _pawn = animator.GetComponent<RoboPawn>();

        _pawn.JumpEvent();
    }
}
