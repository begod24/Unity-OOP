using UnityEngine;

public interface IInteractable
{
    // Called when player presses the interact key (e.g., E) while looking at the object.
    void Interact();
    
    // Optional: hint text for UI (can return null/empty).
    string Hint => string.Empty;
}