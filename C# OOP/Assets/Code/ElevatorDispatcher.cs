using UnityEngine;

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for any dispatching strategy.
/// </summary>
public interface IDispatchStrategy
{
    IElevator PickElevatorFor(HallCall call, IReadOnlyList<IElevator> elevators);
}

/// <summary>
/// Simple strategy – picks the closest elevator by distance.
/// </summary>
[Serializable]
public sealed class NearestByDistanceStrategy : IDispatchStrategy
{
    public float floorTravelTime = 0.8f;
    public float onTheWayBonus = 0.5f;

    public IElevator PickElevatorFor(HallCall call, IReadOnlyList<IElevator> elevators)
    {
        IElevator best = null;
        float bestScore = float.PositiveInfinity;

        foreach (var e in elevators)
        {
            float eta = e.EstimateRoughETA(call.Floor, floorTravelTime);

            // Give a small bonus if elevator is already moving in the same direction and call is along its path.
            bool goingSameDir =
                (e.Direction == call.WantedDirection) &&
                ((call.WantedDirection == DirectionState.Up && call.Floor >= e.CurrentFloorIndex) ||
                 (call.WantedDirection == DirectionState.Down && call.Floor <= e.CurrentFloorIndex));

            if (goingSameDir) eta -= onTheWayBonus;
            if (e.IsFull) eta += 2f;

            if (eta < bestScore)
            {
                bestScore = eta;
                best = e;
            }
        }

        return best;
    }
}

/// <summary>
/// Central dispatcher. Owns the hall call queue and assigns calls to elevators.
/// </summary>
public sealed class ElevatorDispatcher : MonoBehaviour
{
    [SerializeField] private ElevatorManager elevatorManager;
    [SerializeField] private NearestByDistanceStrategy strategy = new NearestByDistanceStrategy();

    private readonly Queue<HallCall> hallQueue = new Queue<HallCall>();

    public event Action<HallCall, IElevator> OnAssigned;
    public event Action<HallCall> OnEnqueued;

    private void Awake()
    {
        if (elevatorManager == null)
            elevatorManager = FindObjectOfType<ElevatorManager>();
    }

    public void EnqueueHallCall(HallCall call)
    {
        hallQueue.Enqueue(call);
        OnEnqueued?.Invoke(call);
        TryAssign();
    }

    private void Update()
    {
        if (hallQueue.Count > 0)
            TryAssign();
    }

    private void TryAssign()
    {
        if (hallQueue.Count == 0) return;
        var elevators = elevatorManager.GetElevators();

        int count = hallQueue.Count;
        for (int i = 0; i < count; i++)
        {
            var call = hallQueue.Dequeue();
            var chosen = strategy.PickElevatorFor(call, elevators);
            if (chosen != null)
            {
                chosen.AssignHallCall(call);
                OnAssigned?.Invoke(call, chosen);
            }
            else
            {
                // No elevator available yet – keep it in queue.
                hallQueue.Enqueue(call);
            }
        }
    }
}