using UnityEngine;

using System;

// Immutable model representing a hall call (button pressed on a floor).
[Serializable]
public sealed class HallCall
{
    public int Floor { get; }
    public DirectionState WantedDirection { get; }
    public DateTime CreatedAt { get; }

    public HallCall(int floor, DirectionState wantedDirection)
    {
        if (wantedDirection == DirectionState.None)
            throw new ArgumentException("HallCall must specify Up or Down.");
        if (floor < 0)
            throw new ArgumentOutOfRangeException(nameof(floor));

        Floor = floor;
        WantedDirection = wantedDirection;
        CreatedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"HallCall[{Floor}, {WantedDirection}]";
}
