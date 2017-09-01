using System;

/* PLAYERSTATE:
 * Data representation of a player instance
 * during play mode runtime. Used as complete
 * reference to a player instance.
 * */
[Serializable]
public class PlayerState
{
    public RoboController controller;
    public int totalWins;

    [NonSerialized] public GameSettings.PlayerInfo playerInfo;

    public bool IsAlive { get { return controller && controller.IsAlive; } }
}