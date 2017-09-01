using System;
using System.Collections.Generic;
using UnityEngine;

/* ROBOCONTROLLER:
 * Monobehaviour that lives on the GameManager GameObject and persists
 * independent of the pawn. Handles input processing before sending action
 * requests to pawn, which controls its own state logic for behaviors.
 * Uses ControllerProfile to specify source inputs, or to run CPU behaviors.
 * */
public class RoboController : MonoBehaviour
{
    public ControlProfile controlProfile;

    [NonSerialized] public PlayerState player;

    public RoboPawn Pawn
    {
        get
        {
            return _pawn;
        }
    }

    private const float bufferExpirationDuration = 1.0f;
    private float lastMovementInput;
    private float pendingMovementInput;
    private InputType pendingActionInput;
    private List<InputSnapshot> inputBuffer;
    private RoboPawn _pawn;

    public bool IsAlive { get { return Pawn && Pawn.gameObject.activeSelf; } }

    void Awake()
    {
        enabled = false;
        inputBuffer = new List<InputSnapshot>();
    }

    void OnEnable()
    {
        if(!controlProfile)
        {
            enabled = false;
            return;
        }
        
        controlProfile.Initialize(this);
    }

    void Update()
    {
        controlProfile.Process(this);

        ConsumePendingInput();

        if (_pawn.TryConsumeAction(GetCandidateAction()))
        {
            DumpBuffer();
        }
    }

    public void Setup(RoboPawn pawn, PlayerState playerState, Transform spawnPoint)
    {
        _pawn = pawn;

        _pawn.transform.position = spawnPoint.position;

        float flipvalue = spawnPoint.rotation.eulerAngles.y < 180f && spawnPoint.rotation.eulerAngles.y > 0f ? 90f : -90f;
        _pawn.Flip(flipvalue);

        foreach(Material m in _pawn.GetMaterials())
        {
            m.SetColor("_OutlineColor", playerState.playerInfo.Color);
        }
        

        player = playerState;
        controlProfile = player.playerInfo.ControllerProfile;
    }

    public void AddMovementInput(float value)
    {
        pendingMovementInput = value;
    }

    public void AddJumpInput()
    {
        pendingActionInput = InputType.Jump;
    }

    public void AddAttackInput()
    {
        pendingActionInput = InputType.Attack;
    }
    
    private void ConsumePendingInput()
    {
        if(pendingActionInput != InputType.None)
        {
            var snapshot = new InputSnapshot(pendingActionInput, pendingMovementInput, Time.time);
            inputBuffer.Add(snapshot);
            pendingActionInput = InputType.None;
        }

        _pawn.ConsumeMovement(pendingMovementInput);
        lastMovementInput = pendingMovementInput;
        if(pendingMovementInput != 0f)
        {
            pendingMovementInput = 0f;
        }
    }

    public ActionType GetCandidateAction()
    {
        ExpireBuffer();
        InputSnapshot input = GetCandidateInput();
        ActionType action = MapInputToAction(input);
        return action;
    }

    private void ExpireBuffer()
    {
        for(int i = inputBuffer.Count - 1; i >= 0; i--)
        {
            if (Time.time - inputBuffer[i].timeStamp > bufferExpirationDuration)
            {
                inputBuffer.Remove(inputBuffer[i]);
            }
        }
    }

    virtual protected InputSnapshot GetCandidateInput()
    {
        InputSnapshot maxSnapshot = new InputSnapshot();
        foreach(var snapshot in inputBuffer)
        {
            if(snapshot.timeStamp > maxSnapshot.timeStamp)
            {
                maxSnapshot = snapshot;
            }
        }
        return maxSnapshot;
    }

    virtual protected ActionType MapInputToAction(InputSnapshot input)
    {
        ActionType result = ActionType.None;
        switch (input.actionInput)
        {
            case InputType.Attack:
                result = ActionType.Attack;
                break;
            case InputType.Jump:
                if (input.movementInput > 0f)
                    result = ActionType.JumpRight;
                else if (input.movementInput < 0f)
                    result = ActionType.JumpLeft;
                else
                    result = ActionType.Jump;
                break;
            default:
                break;
        }

        return result;
    }

    private void DumpBuffer()
    {
        inputBuffer.Clear();
    }
}
