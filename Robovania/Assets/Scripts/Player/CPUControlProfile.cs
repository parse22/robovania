using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Controller Profiles/CPU Controlled")]
public class CPUControlProfile : ControlProfile
{
    public float punchDistance = 1.5f;
    public float punchHeight = 2f;

    public override void Process(RoboController controller)
    {
        GameObject target = GameObject.FindGameObjectsWithTag("Player")
            .OrderBy(go => Vector3.Distance(go.transform.position, controller.Pawn.transform.position))
            .FirstOrDefault(go => go != controller.Pawn.gameObject);

        if(target)
        {
            if(target.transform.position.x > controller.Pawn.transform.position.x)
            {
                controller.AddMovementInput(1f);
            }
            else
            {
                controller.AddMovementInput(-1f);
            }
            if(Mathf.Abs(target.transform.position.x - controller.Pawn.transform.position.x) < punchDistance && Mathf.Abs(target.transform.position.y - controller.Pawn.transform.position.y) < punchHeight)
            {
                controller.AddAttackInput();
            }
        }
    }
}
