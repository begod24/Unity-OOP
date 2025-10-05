using UnityEngine;

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface used by the dispatcher to control elevators indirectly (DIP).
/// </summary>
public interface IElevator
{
    string Id { get; }
    int CurrentFloorIndex { get; }
    DirectionState Direction { get; }
    ElevatorState State { get; }
    bool IsBusy { get; }
    bool IsFull { get; }

    void AssignHallCall(HallCall call);
    void AcceptCarCall(int floor);
    float EstimateRoughETA(int targetFloor, float floorTravelTime);
    IReadOnlyList<int> Debug_GetTargets();
}

/// <summary>
/// Single elevator cabin – controls motion, direction, and internal queues.
/// </summary>
public sealed class Elevator : MonoBehaviour, IElevator
{
    [Header("Identity")]
    [SerializeField] private string id = "A";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;   // meters per second
    [SerializeField] private float floorHeight = 3f; // meters between floors
    [SerializeField] private int currentFloorIndex = 0;

    [Header("Runtime (read-only)")]
    [SerializeField] private ElevatorState state = ElevatorState.Idle;
    [SerializeField] private DirectionState direction = DirectionState.None;

    // Two ordered queues: ascending for Up, descending for Down.
    private readonly SortedSet<int> upQueue = new SortedSet<int>();
    private readonly SortedSet<int> downQueue = new SortedSet<int>(Comparer<int>.Create((a, b) => b.CompareTo(a)));

    // Current active target floor (if moving).
    private int? activeTarget;

    // Observer pattern events.
    public event Action<Elevator> OnQueueChanged;
    public event Action<Elevator, int> OnArrived;

    // ---- IElevator interface ----
    public string Id => id;
    public int CurrentFloorIndex => currentFloorIndex;
    public DirectionState Direction => direction;
    public ElevatorState State => state;
    public bool IsBusy => activeTarget.HasValue || upQueue.Count > 0 || downQueue.Count > 0;
    public bool IsFull { get; private set; } = false;

    public void AssignHallCall(HallCall call)
    {
        if (call == null) return;
        EnqueueTarget(call.Floor);
    }

    public void AcceptCarCall(int floor)
    {
        EnqueueTarget(floor);
    }

    public float EstimateRoughETA(int targetFloor, float floorTravelTime)
    {
        int distance = Mathf.Abs(CurrentFloorIndex - targetFloor);
        return distance * floorTravelTime;
    }

    public IReadOnlyList<int> Debug_GetTargets()
    {
        var list = new List<int>();
        if (activeTarget.HasValue) list.Add(activeTarget.Value);
        list.AddRange(upQueue);
        list.AddRange(downQueue);
        return list;
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    // ------------------------------
    // Core finite state machine (FSM)
    // ------------------------------
    private void Tick(float dt)
    {
        // 1) If there is no active target, choose the next one.
        if (!activeTarget.HasValue)
        {
            PickNextTarget();
            if (!activeTarget.HasValue)
            {
                state = ElevatorState.Idle;
                direction = DirectionState.None;
                return;
            }
        }

        // 2) Move towards the current target.
        int target = activeTarget.Value;
        float targetY = FloorToWorldY(target);
        float currentY = transform.position.y;

        // If already at the target floor – stop and trigger arrival.
        if (Mathf.Approximately(currentY, targetY))
        {
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            currentFloorIndex = target;
            activeTarget = null;
            OnArrived?.Invoke(this, target);
            OnQueueChanged?.Invoke(this);
            state = ElevatorState.Arriving;
            direction = DirectionState.None;
            return;
        }

        float step = moveSpeed * dt * Mathf.Sign(targetY - currentY);
        float newY = currentY + step;

        // Snap to target if we overshoot.
        if ((step > 0f && newY >= targetY) || (step < 0f && newY <= targetY))
            newY = targetY;

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Update visual state and direction.
        if (newY < targetY)
        {
            state = ElevatorState.MovingUp;
            direction = DirectionState.Up;
        }
        else if (newY > targetY)
        {
            state = ElevatorState.MovingDown;
            direction = DirectionState.Down;
        }
    }

    // Adds a target floor to the correct queue.
    private void EnqueueTarget(int floor)
    {
        if (floor == currentFloorIndex && !activeTarget.HasValue)
        {
            // Already at this floor – trigger arrival immediately.
            OnArrived?.Invoke(this, floor);
            OnQueueChanged?.Invoke(this);
            return;
        }

        if (floor > currentFloorIndex)
            upQueue.Add(floor);
        else if (floor < currentFloorIndex)
            downQueue.Add(floor);

        OnQueueChanged?.Invoke(this);
    }

    // Picks the next destination depending on queues and current direction.
    private void PickNextTarget()
    {
        // Continue current direction if possible.
        if (direction == DirectionState.Up && upQueue.Count > 0)
        {
            activeTarget = TakeFirst(upQueue);
            return;
        }
        if (direction == DirectionState.Down && downQueue.Count > 0)
        {
            activeTarget = TakeFirst(downQueue);
            return;
        }

        // If idle, choose the nearest queue.
        if (upQueue.Count > 0 && downQueue.Count == 0)
        {
            direction = DirectionState.Up;
            activeTarget = TakeFirst(upQueue);
            return;
        }

        if (downQueue.Count > 0 && upQueue.Count == 0)
        {
            direction = DirectionState.Down;
            activeTarget = TakeFirst(downQueue);
            return;
        }

        // Choose the closest head between up/down queues.
        if (upQueue.Count > 0 && downQueue.Count > 0)
        {
            int upCandidate = upQueue.Min;
            int downCandidate = downQueue.Min;
            int dUp = Mathf.Abs(upCandidate - currentFloorIndex);
            int dDown = Mathf.Abs(downCandidate - currentFloorIndex);

            if (dUp <= dDown)
            {
                direction = DirectionState.Up;
                activeTarget = TakeFirst(upQueue);
            }
            else
            {
                direction = DirectionState.Down;
                activeTarget = TakeFirst(downQueue);
            }
            return;
        }

        // No targets left.
        activeTarget = null;
        direction = DirectionState.None;
    }

    private static int TakeFirst(SortedSet<int> set)
    {
        int val = set.Min;
        set.Remove(val);
        return val;
    }

    private float FloorToWorldY(int floorIndex) => floorIndex * floorHeight;
}