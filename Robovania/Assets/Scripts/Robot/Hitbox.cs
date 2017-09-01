using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {

    public RoboPawn pawn;

    private void OnTriggerEnter(Collider other)
    {
        pawn.HitboxCollision(other);
    }
}
