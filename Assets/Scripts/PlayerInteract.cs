using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayerMask;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float overlapSphereRadius = 3f;
    [SerializeField] private float raycastSphereRadius = 0.5f;

    private IInteractable currentFocusedInteractable;
    private HashSet<IInteractable> previousNearby = new HashSet<IInteractable>();

    private void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Clean dead focus
        if (!IsAlive(currentFocusedInteractable))
            currentFocusedInteractable = null;

        Vector3 origin = cam.transform.position;
        Vector3 direction = cam.transform.forward;

        // =====================
        // 1. OVERLAP (NEARBY ICONS)
        // =====================
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, overlapSphereRadius, interactableLayerMask);

        HashSet<IInteractable> currentNearby = new HashSet<IInteractable>();

        foreach (Collider col in nearbyColliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();

            if (IsAlive(interactable))
            {
                currentNearby.Add(interactable);
                interactable.ShowInteractableIcon();
            }
        }

        // =====================
        // 2. CLEANUP (HIDE ICONS)
        // =====================
        foreach (IInteractable interactable in previousNearby)
        {
            if (!IsAlive(interactable)) continue;

            if (!currentNearby.Contains(interactable))
            {
                interactable.HideInteractableIcon();
            }
        }

        previousNearby = currentNearby;

        // =====================
        // 3. SPHERECAST (FOCUSED PROMPT)
        // =====================
        if (Physics.SphereCast(origin, raycastSphereRadius, direction, out RaycastHit hit, interactDistance, interactableLayerMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (IsAlive(interactable) && interactable != currentFocusedInteractable)
            {
                currentFocusedInteractable?.HideInteractionPrompt();
                currentFocusedInteractable = interactable;
                currentFocusedInteractable?.ShowInteractionPrompt();
            }

            if (IsAlive(interactable) && Input.GetKeyDown(KeyCode.F))
            {
                interactable.Interact();
            }
        }
        else
        {
            currentFocusedInteractable?.HideInteractionPrompt();
            currentFocusedInteractable = null;
        }
    }

    private static bool IsAlive(IInteractable interactable)
    {
        if (interactable == null) return false;
        return (interactable as MonoBehaviour) != null;
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;

        Transform cam = Camera.main.transform;

        Vector3 origin = cam.position;
        Vector3 direction = cam.forward;
        Vector3 end = origin + direction * interactDistance;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(origin, raycastSphereRadius);
        Gizmos.DrawWireSphere(end, raycastSphereRadius);

        DrawCircleLines(origin, direction, raycastSphereRadius, end);
        DrawCircleLines(end, direction, raycastSphereRadius, origin);
    }

    private void DrawCircleLines(Vector3 center, Vector3 forward, float radius, Vector3 target)
    {
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized * radius;
        Vector3 up = Vector3.Cross(forward, right).normalized * radius;

        Gizmos.DrawLine(center + right, target + right);
        Gizmos.DrawLine(center - right, target - right);
        Gizmos.DrawLine(center + up, target + up);
        Gizmos.DrawLine(center - up, target - up);
    }
}