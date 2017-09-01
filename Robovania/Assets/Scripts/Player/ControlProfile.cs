using UnityEngine;

public abstract class ControlProfile : ScriptableObject
{
    public virtual void Initialize(RoboController controller) { }
    public abstract void Process(RoboController controller);
}
