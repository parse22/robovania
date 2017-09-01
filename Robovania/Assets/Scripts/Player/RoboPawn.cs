using System.Collections.Generic;
using UnityEngine;

/* ROBOPAWN:
 * Primary Monobehaviour that lives on the robot GameObject
 * Takes action directives, restricted by pawn state logic
 * and applies those actions to the behavior state machine as
 * bound. Runs state update off RoboController Update timing,
 * does minimal behavior in Update, most movement in FixedUpdate
 * since it's physics driven movement through a rigidbody.
 * 
 * Bulk of behavior is driven by enum-backed state machine that
 * is largely identical to the Animator State Machine, but with some
 * differences.
 * */
public class RoboPawn : MonoBehaviour
{

    public float maxSpeed = 5f;
    public float acceleration = 3f;
    public float groundDeceleration = 10f;
    public float turnTime = 0.5f;
    public float jumpSpeed = 200f;
    public float knockbackForce = 50f;
    public PhysicMaterial groundedPhys;
    public PhysicMaterial slidingPhys;
    public BoxCollider hitbox;
    public BoxCollider hurtbox;
    public BoxCollider shockwave;
    public List<Renderer> renderers;

    private MoveState lastState = MoveState.Idle;
    private MoveState currentState = MoveState.Idle;
    private ActionType pendingAction;
    private int direction = 1;
    private int jumpDirection = 0;
    private float turnStartTime;
    private float moveValue;
    private Vector3 intendedVelocity;
    private bool grounded = false;
    private Transform groundCheck;
    private Animator animComponent;
    private Rigidbody rigidBody;
    private CapsuleCollider capsule;
    private float stateTransitionTime;

    void Awake()
    {
        groundCheck = transform.Find("groundCheck");
    }
    void Start()
    {
        animComponent = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        DisableHitbox();
        ShockwaveDisable();
    }

    void Update()
    {
        grounded = Physics.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));

        capsule.material = grounded ? groundedPhys : slidingPhys;
    }

    void FixedUpdate()
    {
        if (currentState == MoveState.Walking)
        {
            if (moveValue * rigidBody.velocity.x < maxSpeed)
            {
                rigidBody.AddForce(Vector3.right * moveValue * acceleration);
            }
            intendedVelocity = Mathf.Abs(rigidBody.velocity.x) < maxSpeed ? rigidBody.velocity : new Vector3(Mathf.Sign(rigidBody.velocity.x) * maxSpeed, rigidBody.velocity.y, 0f);
            rigidBody.velocity = intendedVelocity;
        }
        else
        {
            intendedVelocity = Vector3.zero;
        }

        float rate = Mathf.Abs(intendedVelocity.x / maxSpeed);
        rate = rate < 0.8f ? rate : 1f;
        animComponent.SetFloat("Speed", rate);
    }

    private void StateUpdate()
    {
        switch (currentState)
        {
            case MoveState.Idle:
                IdleUpdate();
                break;
            case MoveState.Walking:
                WalkingUpdate();
                break;
            case MoveState.Turning:
                TurningUpdate();
                break;
            case MoveState.Jumping:
                JumpingUpdate();
                break;
            case MoveState.Rising:
                RisingUpdate();
                break;
            case MoveState.Falling:
                FallingUpdate();
                break;
            case MoveState.Landing:
                LandingUpdate();
                break;
            case MoveState.Attacking:
                AttackUpdate();
                break;
            case MoveState.Stunned:
                StunnedUpdate();
                break;
            default:
                break;
        }
    }

    public void ConsumeMovement(float value)
    {
        moveValue = value;
    }

    public bool TryConsumeAction(ActionType action)
    {
        bool result = false;
        switch (currentState)
        {
            case MoveState.Idle:
                pendingAction = action;
                result = true;
                break;
            case MoveState.Turning:
                result = false;
                break;
            case MoveState.Walking:
                pendingAction = action;
                result = true;
                break;
            case MoveState.Jumping:
                result = false;
                break;
            case MoveState.Landing:
                pendingAction = action;
                result = true;
                break;
            case MoveState.Attacking:
                result = false;
                break;
            case MoveState.Stunned:
                result = false;
                break;
            default:
                break;
        }
        StateUpdate();
        return result;
    }

    private void IdleUpdate()
    {
        if(pendingAction == ActionType.Jump)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = 0;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.JumpLeft)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = -1;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.JumpRight)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = 1;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.Attack)
        {
            GotoState(MoveState.Attacking);
            animComponent.SetTrigger("Punch");
        }
        else if(!grounded)
        {
            GotoState(MoveState.Falling);
            animComponent.SetTrigger("Fall");
        }
        else if (direction == -1)
        {
            if (moveValue > 0f)
            {
                GotoState(MoveState.Turning);
                turnStartTime = Time.time;
                direction = 1;
            }
            if (moveValue < 0f)
            {
                GotoState(MoveState.Walking);
            }
        }
        else
        {
            if (moveValue < 0f)
            {
                GotoState(MoveState.Turning);
                turnStartTime = Time.time;
                direction = -1;
            }
            else if (moveValue > 0f)
            {
                GotoState(MoveState.Walking);
            }
        }
    }

    private void WalkingUpdate()
    {
        if (pendingAction == ActionType.Jump)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = 0;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.JumpLeft)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = -1;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.JumpRight)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = 1;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.Attack)
        {
            GotoState(MoveState.Attacking);
            animComponent.SetTrigger("Punch");
        }
        else if(!grounded)
        {
            GotoState(MoveState.Falling);
            animComponent.SetTrigger("Fall");
        }
        else if (moveValue > 0f)
        {
            if (direction == -1)
            {
                GotoState(MoveState.Turning);
                turnStartTime = Time.time;
                direction = 1;
            }
        }
        else if (moveValue < 0f)
        {
            if (direction == 1)
            {
                GotoState(MoveState.Turning);
                turnStartTime = Time.time;
                direction = -1;
            }
        }
        else
        {
            GotoState(MoveState.Idle);
        }
    }

    private void TurningUpdate()
    {
        if(Time.time - turnStartTime > turnTime)
        {
            transform.rotation = Quaternion.AngleAxis(direction * 90f, Vector3.up);
            GotoState(MoveState.Idle);
        }
        else
        {
            float alpha = (Time.time - turnStartTime) / turnTime;
            transform.rotation = Quaternion.AngleAxis(Mathf.Lerp(-1f * direction * 90f, direction * 90f, alpha), Vector3.up);
        }
    }

    private void JumpingUpdate()
    {
        
    }

    private void RisingUpdate()
    {
        if (!grounded)
        {
            GotoState(MoveState.Falling);
        }
        else if (Time.time - stateTransitionTime > 0.2f)
        {
            GotoState(MoveState.Landing);
            animComponent.SetTrigger("Land");
        }
    }

    private void FallingUpdate()
    {
        if (grounded)
        {
            GotoState(MoveState.Landing);
            animComponent.SetTrigger("Land");
            //velocity.y = 0f;
        }
    }

    private void LandingUpdate()
    {
        if (pendingAction == ActionType.Jump)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = 0;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.JumpLeft)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = -1;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.JumpRight)
        {
            GotoState(MoveState.Jumping);
            jumpDirection = 1;
            animComponent.SetTrigger("Jump");
        }
        else if (pendingAction == ActionType.Attack)
        {
            currentState = MoveState.Attacking;
            animComponent.SetTrigger("Punch");
        }
    }

    private void AttackUpdate()
    {

    }

    private void StunnedUpdate()
    {

    }

    public void Flip(float value)
    {
        transform.rotation = Quaternion.AngleAxis(value, Vector3.up);
        direction = value > 0 ? 1 : -1;
    }

    public void JumpEvent()
    {
        if(currentState == MoveState.Jumping)
        {
            float forwardBias = jumpDirection == direction ? 0.8f : 0.4f;
            rigidBody.velocity = jumpSpeed * (Vector3.up + (Vector3.right * jumpDirection * forwardBias));
            GotoState(MoveState.Rising);
        }
    }

    public void LandedEvent()
    {
        if(currentState == MoveState.Landing)
        {
            GotoState(MoveState.Idle);
        }
    }

    public void EnableHitbox()
    {
        hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }

    public void ShockwaveEnable()
    {
        shockwave.enabled = true;
        GetComponent<AudioSource>().Play();
    }

    public void ShockwaveDisable()
    {
        shockwave.enabled = false;
    }

    public void HitboxCollision(Collider other)
    {
        Hurtbox victim = other.GetComponent<Hurtbox>();
        if (victim)
        {
            Vector3 knockback = victim.pawn.transform.position - transform.position;
            knockback.Normalize();
            knockback += Vector3.up;
            knockback.Normalize();
            knockback *= knockbackForce;
            victim.pawn.GetComponent<Rigidbody>().AddForce(knockback);
            GetComponent<AudioSource>().Play();
        }

    }

    public void AttackEndEvent()
    {
        if(currentState == MoveState.Attacking)
        {
            GotoState(MoveState.Idle);
        }
    }

    private void GotoState(MoveState nextState)
    {
        lastState = currentState;
        currentState = nextState;
        stateTransitionTime = Time.time;
    }

    public List<Material> GetMaterials()
    {
        List<Material> result = new List<Material>();
        foreach(Renderer r in renderers)
        {
            result.AddRange(r.materials);
        }
        return result;
    }
}
