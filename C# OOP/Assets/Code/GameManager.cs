using UnityEngine;

/// <summary>
/// Root object of the scene – connects managers and initializes references.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private ElevatorManager elevatorManager;
    [SerializeField] private ElevatorDispatcher dispatcher;

    [System.Obsolete]
    private void Awake()
    {
        if (elevatorManager == null) elevatorManager = FindObjectOfType<ElevatorManager>();
        if (dispatcher == null) dispatcher = FindObjectOfType<ElevatorDispatcher>();
    }
}

