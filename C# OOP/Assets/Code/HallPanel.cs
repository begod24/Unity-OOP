using UnityEngine;

/// <summary>
/// Floor panel – creates hall calls when Up or Down buttons are pressed.
/// </summary>
public sealed class HallPanel : MonoBehaviour
{
    [SerializeField] private int floorIndex = 0;
    [SerializeField] private ElevatorDispatcher dispatcher;

    [System.Obsolete]
    private void Awake()
    {
        if (dispatcher == null)
            dispatcher = FindObjectOfType<ElevatorDispatcher>();
    }

    // These methods are meant to be called by UI Buttons.
    public void CallUp()
    {
        var call = new HallCall(floorIndex, DirectionState.Up);
        dispatcher.EnqueueHallCall(call);
    }

    public void CallDown()
    {
        var call = new HallCall(floorIndex, DirectionState.Down);
        dispatcher.EnqueueHallCall(call);
    }
}
