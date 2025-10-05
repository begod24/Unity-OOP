using UnityEngine;

using System.Collections.Generic;

/// <summary>
/// Holds references to all elevators and exposes them as IElevator for the dispatcher.
/// </summary>
public sealed class ElevatorManager : MonoBehaviour
{
    [SerializeField] private List<Elevator> elevators = new List<Elevator>();

    public IReadOnlyList<IElevator> GetElevators()
    {
        return elevators.ConvertAll<IElevator>(e => e);
    }

    public void Register(Elevator e)
    {
        if (!elevators.Contains(e))
            elevators.Add(e);
    }
}
