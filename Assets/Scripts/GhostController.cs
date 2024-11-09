using UnityEngine;
public class GhostController : MonoBehaviour
{
    public bool IsScared { get; private set; }

    public void TransitionToScaredState()
    {
        IsScared = true;
        // Trigger "Scared" animation
    }

    public void TransitionToWalkingState()
    {
        IsScared = false;
        // Trigger "Walking" animation
    }

    public void TransitionToDeadState()
    {
        // Trigger "Dead" animation
    }
    public void TransitionToRecoveringState()
    {
        IsScared = false;
        // Trigger "Recovering" animation and state logic
    }
}
