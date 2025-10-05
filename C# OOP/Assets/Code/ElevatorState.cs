using UnityEngine;

// High-level finite states for an elevator.
public enum ElevatorState
{
    Idle,
    MovingUp,
    MovingDown,
    Arriving
}