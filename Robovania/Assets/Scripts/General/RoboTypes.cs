using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveState { Idle, Walking, Turning, Jumping, Rising, Falling, Landing, Attacking, Stunned }
public enum ActionType { None, Attack, Jump, JumpLeft, JumpRight }
public enum InputType { None, Attack, Jump }

public struct InputSnapshot
{
    public InputType actionInput;
    public float movementInput;
    public float timeStamp;

    public InputSnapshot(InputType action, float movement, float time)
    {
        actionInput = action;
        movementInput = movement;
        timeStamp = time;
    }
}
