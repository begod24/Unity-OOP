using UnityEngine;
using UnityEngine.UI;

// Casts a ray from camera center to find IInteractable targets.
// Shows a crosshair and changes its color when aim is valid.
public sealed class InteractRaycaster : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    [SerializeField] private Image crosshair;         // small UI Image at screen center
    [SerializeField] private Color idleColor = new Color(1f,1f,1f,0.6f);
    [SerializeField] private Color canInteractColor = Color.green;

    private Camera cam;
    private IInteractable current;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (crosshair != null) crosshair.color = idleColor;
    }

    private void Update()
    {
        UpdateAim();
        if (current != null && Input.GetKeyDown(interactKey))
            current.Interact();
    }

    private void UpdateAim()
    {
        current = null;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            current = hit.collider.GetComponent<IInteractable>();
            // If the IInteractable is on a parent, try GetComponentInParent as well:
            if (current == null)
                current = hit.collider.GetComponentInParent<IInteractable>();
        }

        if (crosshair != null)
            crosshair.color = current != null ? canInteractColor : idleColor;
    }
}

