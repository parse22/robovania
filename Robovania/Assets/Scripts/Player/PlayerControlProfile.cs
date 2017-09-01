using UnityEngine;

[CreateAssetMenu(menuName="Controller Profiles/Player Controlled")]
public class PlayerControlProfile : ControlProfile
{
    public int playerNumber;

    private string movementAxisName;
    private string jumpButton;
    private string attackButton;

    public void OnEnable()
    {
        movementAxisName = "Horizontal" + playerNumber;
        jumpButton = "Jump" + playerNumber;
        attackButton = "Attack" + playerNumber;
    }

    public override void Process(RoboController controller)
    {
        float axisValue = Input.GetAxis(movementAxisName);
        if (Mathf.Abs(axisValue) > 0f)
        {
            controller.AddMovementInput(axisValue);
        }

        if(Input.GetButtonDown(jumpButton))
        {
            controller.AddJumpInput();
        }
                
        if(Input.GetButtonDown(attackButton))
        {
            controller.AddAttackInput();
        }
    }
}
